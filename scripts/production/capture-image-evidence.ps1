param(
    [string]$EvidenceDirectory = 'artifacts/release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')
Push-Location $repoRoot
try {
    $services = @('appcondominio-api', 'appcondominio-worker', 'appcondominio-web')
    $configuredImages = @(& docker compose config --images)
    if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve Compose image names.' }

    $images = foreach ($service in $services) {
        $imageName = @($configuredImages | Where-Object { $_ -eq $service -or $_.EndsWith("-$service") })
        if ($imageName.Count -ne 1) {
            throw "Expected exactly one built image for service '$service', found $($imageName.Count)."
        }

        $contentDigest = (& docker image inspect $imageName[0] --format '{{.Id}}').Trim()
        if ($LASTEXITCODE -ne 0 -or $contentDigest -notmatch '^sha256:[a-f0-9]{64}$') {
            throw "Invalid immutable image digest for service '$service'."
        }

        $repoDigestsRaw = (& docker image inspect $imageName[0] --format '{{json .RepoDigests}}').Trim()
        $repoDigests = if ($repoDigestsRaw -and $repoDigestsRaw -ne 'null') {
            @($repoDigestsRaw | ConvertFrom-Json)
        } else { @() }

        [ordered]@{
            service = $service
            contentDigest = $contentDigest
            repositoryDigests = $repoDigests
        }
    }
    $commitSha = (git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $commitSha -notmatch '^[a-f0-9]{40}$') {
        throw 'Unable to resolve release commit SHA.'
    }

    New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
    $evidencePath = Join-Path $EvidenceDirectory 'image-evidence.json'
    [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString('o')
        commitSha = $commitSha
        images = @($images)
        result = 'PASS'
    } | ConvertTo-Json -Depth 6 | Set-Content -Path $evidencePath -Encoding utf8

    Write-Host "Immutable image evidence captured: $evidencePath"
}
finally {
    Pop-Location
}
