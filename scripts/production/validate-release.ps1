param(
    [string]$EnvironmentFile = '.env'
)

$ErrorActionPreference = 'Stop'

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

Write-Host 'Validating AppCondominio release prerequisites...'

Assert-Condition (Test-Path 'docker-compose.yml') 'docker-compose.yml is missing.'
Assert-Condition (Test-Path '.env.example') '.env.example is missing.'
Assert-Condition (Test-Path 'AppCondominio.sln') 'AppCondominio.sln is missing.'
Assert-Condition (Test-Path 'infrastructure/portal/security/appcondominio-security-registration.json') 'Portal security manifest is missing.'
Assert-Condition (Test-Path 'infrastructure/portal/menu/appcondominio-menu-registration.json') 'Portal menu manifest is missing.'

$securityManifest = Get-Content 'infrastructure/portal/security/appcondominio-security-registration.json' -Raw | ConvertFrom-Json
Assert-Condition ($securityManifest.resources.Count -ge 1) 'Portal security manifest has no resources.'
Assert-Condition ($securityManifest.permissions.Count -ge 2) 'Portal security manifest has no permissions.'

if (Test-Path $EnvironmentFile) {
    $envText = Get-Content $EnvironmentFile -Raw
    $forbidden = @('ChangeMeOutsideGit', 'JWT_SECRET=$', 'JWT_SECRET=\s*$')
    foreach ($pattern in $forbidden) {
        if ($envText -match $pattern) { throw "Unsafe placeholder detected in ${EnvironmentFile}: $pattern" }
    }
}

$docker = Get-Command docker -ErrorAction SilentlyContinue
if ($null -ne $docker) {
    docker compose config --quiet
    if ($LASTEXITCODE -ne 0) { throw 'docker compose config validation failed.' }
} else {
    Write-Warning 'Docker is not installed in this execution environment; compose validation skipped.'
}

Write-Host 'Release prerequisite validation passed.'
