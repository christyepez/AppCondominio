param(
    [switch]$SkipBuild,
    [string]$EvidenceDirectory = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
. (Join-Path $PSScriptRoot 'migration-contexts.ps1')

Push-Location $repoRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }

    if (-not $SkipBuild) {
        dotnet restore AppCondominio.sln
        if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }
        dotnet build AppCondominio.sln -c Release --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }
    }

    $results = @()
    foreach ($item in $MigrationContexts) {
        Write-Host "Checking $($item.Context)..."
        $output = & dotnet ef migrations list --project $item.Project --startup-project $item.Project --context $item.Context --configuration Release --no-connect 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Migration listing failed for $($item.Context): $($output -join [Environment]::NewLine)" }
        $migrations = @($output | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ -match '^\d{14}_.+' })
        if ($migrations.Count -eq 0) { throw "No migrations found for $($item.Context)." }

        $results += [pscustomobject]@{
            module = $item.Module
            context = $item.Context
            historySchema = $item.Schema
            migrationCount = $migrations.Count
            migrations = $migrations
            status = 'PASS'
        }
    }

    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $stamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ')
    $evidencePath = Join-Path $EvidenceDirectory "migration-precheck-$stamp.json"
    [pscustomobject]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        commitSha = (git rev-parse HEAD).Trim()
        contextCount = $results.Count
        result = 'PASS'
        contexts = $results
    } | ConvertTo-Json -Depth 6 | Set-Content -Path $evidencePath -Encoding utf8

    Write-Host "Migration precheck passed for $($results.Count) contexts. Evidence: $evidencePath"
}
finally { Pop-Location }
