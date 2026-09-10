param(
    [string]$EnvironmentFile = '.env'
)

$ErrorActionPreference = 'Stop'

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

Write-Host 'Validating AppCondominio release prerequisites...'

Assert-Condition (Test-Path 'docker-compose.yml') 'docker-compose.yml is missing.'
Assert-Condition (Test-Path 'docker-compose.hub.yml') 'docker-compose.hub.yml is missing.'
Assert-Condition (Test-Path '.env.example') '.env.example is missing.'
Assert-Condition (Test-Path 'AppCondominio.sln') 'AppCondominio.sln is missing.'
Assert-Condition (Test-Path 'scripts/local/start-dockerhub.ps1') 'Docker Hub runtime launcher is missing.'
Assert-Condition (Test-Path 'scripts/database/migration-contexts.ps1') 'Migration context inventory is missing.'
Assert-Condition (Test-Path 'scripts/database/migration-precheck.ps1') 'Migration precheck script is missing.'
Assert-Condition (Test-Path 'scripts/database/apply-migrations.ps1') 'Migration apply script is missing.'
Assert-Condition (Test-Path 'infrastructure/portal/security/appcondominio-security-registration.json') 'Portal security manifest is missing.'
Assert-Condition (Test-Path 'infrastructure/portal/menu/appcondominio-menu-registration.json') 'Portal menu manifest is missing.'
Assert-Condition (Test-Path 'scripts/database/backup-restore-smoke.ps1') 'Backup/restore rehearsal script is missing.'

$securityManifest = Get-Content 'infrastructure/portal/security/appcondominio-security-registration.json' -Raw | ConvertFrom-Json
Assert-Condition ($securityManifest.resources.Count -ge 1) 'Portal security manifest has no resources.'
Assert-Condition ($securityManifest.permissions.Count -ge 2) 'Portal security manifest has no permissions.'

if (Test-Path $EnvironmentFile) {
    $envText = Get-Content $EnvironmentFile -Raw
    $forbidden = @('ChangeMeOutsideGit', '(?m)^JWT_SECRET=\s*$', '(?m)^SEQ_ADMIN_PASSWORD=\s*$')
    foreach ($pattern in $forbidden) {
        if ($envText -match $pattern) { throw "Unsafe placeholder detected in ${EnvironmentFile}: $pattern" }
    }

    $requiredHubKeys = @('APPCONDOMINIO_API_IMAGE','APPCONDOMINIO_WORKER_IMAGE','APPCONDOMINIO_WEB_IMAGE')
    foreach ($key in $requiredHubKeys) {
        $match = [regex]::Match($envText, "(?m)^$key=(.+)$")
        Assert-Condition ($match.Success) "Missing $key in $EnvironmentFile."
        Assert-Condition ($match.Groups[1].Value.Trim() -match '@sha256:[0-9a-f]{64}$') "$key must use an immutable Docker Hub digest."
    }
}

$docker = Get-Command docker -ErrorAction SilentlyContinue
if ($null -ne $docker) {
    if (Test-Path $EnvironmentFile) {
        docker compose --env-file $EnvironmentFile config --quiet
        if ($LASTEXITCODE -ne 0) { throw 'docker compose config validation failed.' }
        docker compose -f docker-compose.yml -f docker-compose.hub.yml --env-file $EnvironmentFile config --quiet
        if ($LASTEXITCODE -ne 0) { throw 'Docker Hub compose overlay validation failed.' }
    } else {
        docker compose config --quiet
        if ($LASTEXITCODE -ne 0) { throw 'docker compose config validation failed.' }
    }
} else {
    Write-Warning 'Docker is not installed in this execution environment; compose validation skipped.'
}

Write-Host 'Release prerequisite validation passed.'