param(
    [switch]$Apply,
    [Parameter(Mandatory = $true)][string]$BackupEvidencePath,
    [string[]]$Context,
    [string]$EvidenceDirectory = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
. (Join-Path $PSScriptRoot 'migration-contexts.ps1')

if (-not $Apply) {
    throw 'Refusing to modify databases. Re-run with -Apply after backup evidence is available.'
}
if (-not (Test-Path $BackupEvidencePath)) {
    throw "Backup evidence not found: $BackupEvidencePath"
}

$selected = $MigrationContexts
if ($Context -and $Context.Count -gt 0) {
    $selected = @($MigrationContexts | Where-Object { $Context -contains $_.Context })
    if ($selected.Count -ne $Context.Count) { throw 'One or more requested DbContext names are not in the approved migration inventory.' }
}

foreach ($item in $selected) {
    $key = "ConnectionStrings__$($item.Module)"
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($key))) {
        throw "Explicit environment variable $key is required for migration apply."
    }
}
Push-Location $repoRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }
    dotnet restore AppCondominio.sln
    if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }
    dotnet build AppCondominio.sln -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }

    $results = @()
    foreach ($item in $selected) {
        Write-Host "Applying migrations for $($item.Context)..."
        $output = & dotnet ef database update --project $item.Project --startup-project $item.Project --context $item.Context --configuration Release 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Migration apply failed for $($item.Context): $($output -join [Environment]::NewLine)" }
        $results += [pscustomobject]@{ module=$item.Module; context=$item.Context; historySchema=$item.Schema; status='PASS' }
    }

    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $stamp=(Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
    $evidencePath=Join-Path $EvidenceDirectory "migration-apply-$stamp.json"
    [pscustomobject]@{
        generatedAtUtc=(Get-Date).ToUniversalTime().ToString('o')
        commitSha=(git rev-parse HEAD).Trim()
        backupEvidence=(Get-Item $BackupEvidencePath).Name
        result='PASS'
        contexts=$results
    } | ConvertTo-Json -Depth 5 | Set-Content -Path $evidencePath -Encoding utf8
    Write-Host "Migration apply passed for $($results.Count) contexts. Evidence: $evidencePath"
}
finally { Pop-Location }
