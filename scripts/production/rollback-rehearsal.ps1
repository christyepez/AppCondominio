param(
    [string]$PreviousCommit = 'HEAD^',
    [string]$EvidenceDirectory = 'artifacts/release',
    [int]$ReadyExpectedStatus = 503
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Push-Location $repoRoot
$tempWorktree = $null
try {
    $currentSha = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $currentSha -notmatch '^[a-f0-9]{40}$') {
        throw 'Unable to resolve current release SHA.'
    }

    $previousExpression = $PreviousCommit
    if ($PreviousCommit -eq 'HEAD^') {
        $parentTokens = ((& git rev-list --parents -n 1 HEAD).Trim() -split '\s+')
        if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect current release commit ancestry.' }

        # pull_request workflows check out GitHub's synthetic merge commit. Only in
        # that event shape should the second parent be treated as the PR source head
        # and rolled back to its parent. A normal push can itself point at a real
        # merge commit; in that case HEAD^ (the first parent) is the correct previous
        # release-capable revision and must not be replaced by the second-parent path.
        $isPullRequestEvent = $env:GITHUB_EVENT_NAME -eq 'pull_request'
        if ($isPullRequestEvent -and $parentTokens.Count -ge 3) {
            $sourceHeadSha = $parentTokens[2]
            & git cat-file -e "$sourceHeadSha^" 2>$null
            if ($LASTEXITCODE -ne 0) {
                & git fetch --no-tags --depth=2 origin $sourceHeadSha
                if ($LASTEXITCODE -ne 0) { throw 'Unable to fetch PR source history for rollback rehearsal.' }
            }
            $previousExpression = "$sourceHeadSha^"
        }
    }

    $previousSha = (& git rev-parse $previousExpression).Trim()
    if ($LASTEXITCODE -ne 0 -or $previousSha -notmatch '^[a-f0-9]{40}$') {
        throw 'Unable to resolve previous release SHA.'
    }
    if ($currentSha -eq $previousSha) { throw 'Rollback target must differ from current release SHA.' }
    if (-not (Test-Path '.env')) { throw 'Rollback rehearsal requires the prepared .env file.' }

    & docker compose down -v --remove-orphans
    if ($LASTEXITCODE -ne 0) { throw 'Unable to stop current runtime stack before rollback rehearsal.' }

    $tempWorktree = Join-Path ([IO.Path]::GetTempPath()) ("appcondominio-rollback-{0}" -f ([Guid]::NewGuid().ToString('N')))
    & git worktree add --detach $tempWorktree $previousSha
    if ($LASTEXITCODE -ne 0) { throw 'Unable to create rollback worktree.' }
    Copy-Item '.env' (Join-Path $tempWorktree '.env') -Force

    Push-Location $tempWorktree
    try {
        & docker compose build appcondominio-api appcondominio-worker appcondominio-web
        if ($LASTEXITCODE -ne 0) { throw 'Unable to build previous immutable image set.' }
        $configuredImages = @(& docker compose config --images)
        if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve previous Compose image names.' }
        $services = @('appcondominio-api', 'appcondominio-worker', 'appcondominio-web')
        $previousImages = foreach ($service in $services) {
            $imageName = @($configuredImages | Where-Object { $_ -eq $service -or $_.EndsWith("-$service") })
            if ($imageName.Count -ne 1) { throw "Expected one rollback image for '$service'." }
            $digest = (& docker image inspect $imageName[0] --format '{{.Id}}').Trim()
            if ($LASTEXITCODE -ne 0 -or $digest -notmatch '^sha256:[a-f0-9]{64}$') {
                throw "Invalid rollback image digest for '$service'."
            }
            [ordered]@{ service = $service; contentDigest = $digest }
        }

        & docker compose up -d
        if ($LASTEXITCODE -ne 0) { throw 'Unable to start previous runtime stack.' }
        & (Join-Path $repoRoot 'scripts/production/smoke-test.ps1') -ReadyExpectedStatus $ReadyExpectedStatus
        if ($LASTEXITCODE -ne 0) { throw 'Rollback runtime smoke failed.' }
    }
    finally {
        & docker compose down -v --remove-orphans | Out-Null
        Pop-Location
    }

    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $evidencePath = Join-Path $EvidenceDirectory ("rollback-rehearsal-{0}.json" -f (Get-Date).ToUniversalTime().ToString('yyyyMMddTHHmmssZ'))
    [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        currentCommitSha = $currentSha
        rollbackCommitSha = $previousSha
        readinessExpectedStatus = $ReadyExpectedStatus
        rollbackImages = @($previousImages)
        result = 'PASS'
    } | ConvertTo-Json -Depth 5 | Set-Content -Path $evidencePath -Encoding utf8

    Write-Host "Runtime rollback rehearsal passed. Evidence: $evidencePath"
}
finally {
    if ($tempWorktree) {
        try { & git worktree remove --force $tempWorktree | Out-Null } catch { Write-Warning $_.Exception.Message }
        try { & git worktree prune | Out-Null } catch { Write-Warning $_.Exception.Message }
    }
    Pop-Location
}
