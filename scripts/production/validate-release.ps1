param(
    [string]$EnvironmentFile = '.env'
)

$ErrorActionPreference = 'Stop'
function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

Write-Host 'Validating AppCondominio release prerequisites...'
$required = @(
    'docker-compose.yml',
    'docker-compose.hub.yml',
    '.env.example',
    'AppCondominio.sln',
    'scripts/local/start-dockerhub.ps1',
    'scripts/local/start-dockerhub.cmd',
    'scripts/database/migration-contexts.ps1',
    'scripts/database/migration-precheck.ps1',
    'scripts/database/apply-migrations.ps1',
    'scripts/database/backup-restore-smoke.ps1',
    'infrastructure/portal/security/appcondominio-security-registration.json',
    'infrastructure/portal/menu/appcondominio-menu-registration.json'
)
foreach ($path in $required) { Assert-Condition (Test-Path $path) "Required release artifact missing: $path" }

$securityManifest = Get-Content 'infrastructure/portal/security/appcondominio-security-registration.json' -Raw | ConvertFrom-Json
Assert-Condition ($securityManifest.resources.Count -ge 1) 'Portal security manifest has no resources.'
Assert-Condition ($securityManifest.permissions.Count -ge 2) 'Portal security manifest has no permissions.'

$hubKeys = @('APPCONDOMINIO_API_IMAGE','APPCONDOMINIO_WORKER_IMAGE','APPCONDOMINIO_WEB_IMAGE')
$hubValues = @{}
if (Test-Path $EnvironmentFile) {
    $envText = Get-Content $EnvironmentFile -Raw
    foreach ($pattern in @('ChangeMeOutsideGit', '(?m)^JWT_SECRET=\s*$', '(?m)^SEQ_ADMIN_PASSWORD=\s*$')) {
        if ($envText -match $pattern) { throw "Unsafe placeholder detected in ${EnvironmentFile}: $pattern" }
    }
    foreach ($key in $hubKeys) {
        $match = [regex]::Match($envText, "(?m)^$key=(.*)$")
        $hubValues[$key] = if ($match.Success) { $match.Groups[1].Value.Trim() } else { '' }
    }
    $configuredCount = @($hubKeys | Where-Object { $hubValues[$_] }).Count
    if ($configuredCount -gt 0) {
        Assert-Condition ($configuredCount -eq $hubKeys.Count) 'Docker Hub image references must be configured as a complete set.'
        foreach ($key in $hubKeys) { Assert-Condition ($hubValues[$key] -match '@sha256:[0-9a-f]{64}$') "$key must use an immutable Docker Hub digest." }
    }
}
$docker = Get-Command docker -ErrorAction SilentlyContinue
if ($null -ne $docker) {
    if (Test-Path $EnvironmentFile) { docker compose --env-file $EnvironmentFile config --quiet }
    else { docker compose config --quiet }
    if ($LASTEXITCODE -ne 0) { throw 'docker compose config validation failed.' }

    $previous = @{}
    $validationDigest = 'sha256:' + ('0' * 64)
    try {
        foreach ($key in $hubKeys) {
            $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
            [Environment]::SetEnvironmentVariable($key, "validation/$($key.ToLowerInvariant())@$validationDigest", 'Process')
        }
        if (Test-Path $EnvironmentFile) { docker compose -f docker-compose.yml -f docker-compose.hub.yml --env-file $EnvironmentFile config --quiet }
        else { docker compose -f docker-compose.yml -f docker-compose.hub.yml config --quiet }
        if ($LASTEXITCODE -ne 0) { throw 'Docker Hub compose overlay validation failed.' }
    }
    finally {
        foreach ($key in $hubKeys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
    }
} else {
    Write-Warning 'Docker is not installed in this execution environment; compose validation skipped.'
}

Write-Host 'Release prerequisite validation passed.'
