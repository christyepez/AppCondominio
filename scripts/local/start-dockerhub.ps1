param(
  [string]$EnvironmentFile = '.env',
  [int]$ReadyExpectedStatus = 503,
  [switch]$SkipPull
)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path $EnvironmentFile)) { throw "Environment file not found: $EnvironmentFile" }
$envMap = @{}
Get-Content $EnvironmentFile | ForEach-Object { if ($_ -match '^\s*([^#=]+)=(.*)$') { $envMap[$matches[1].Trim()] = $matches[2].Trim() } }
$keys = @('APPCONDOMINIO_API_IMAGE','APPCONDOMINIO_WORKER_IMAGE','APPCONDOMINIO_WEB_IMAGE')
foreach ($key in $keys) {
  if (-not $envMap.ContainsKey($key) -or $envMap[$key] -notmatch '@sha256:[0-9a-f]{64}$') { throw "$key must use an immutable Docker Hub digest." }
}
$appPort = if ($envMap['APP_HTTP_PORT']) { $envMap['APP_HTTP_PORT'] } else { '8088' }
$webPort = if ($envMap['WEB_HTTP_PORT']) { $envMap['WEB_HTTP_PORT'] } else { '4208' }
$files = @('-f','docker-compose.yml','-f','docker-compose.hub.yml')
if (-not $SkipPull) {
  docker manifest inspect $envMap['APPCONDOMINIO_API_IMAGE'] *> $null
  if ($LASTEXITCODE -ne 0) { throw 'Docker Hub authentication/registry check failed. Run docker login and retry.' }
  docker compose @files --env-file $EnvironmentFile pull appcondominio-api appcondominio-worker appcondominio-web
  if ($LASTEXITCODE -ne 0) { throw 'Docker Hub image pull failed.' }
}
docker compose @files --env-file $EnvironmentFile config --quiet
if ($LASTEXITCODE -ne 0) { throw 'Docker Hub compose validation failed.' }
docker compose @files --env-file $EnvironmentFile up -d --no-build
if ($LASTEXITCODE -ne 0) { throw 'Docker Hub runtime startup failed.' }
$expected = @('sqlserver','redis','rabbitmq','seq','appcondominio-api','appcondominio-worker','appcondominio-web')
$running = @(docker compose @files --env-file $EnvironmentFile ps --status running --services)
$missing = @($expected | Where-Object { $_ -notin $running })
if ($missing.Count -gt 0) { throw "Services not running: $($missing -join ', ')" }
& ./scripts/production/smoke-test.ps1 -BaseUrl "http://localhost:$appPort" -WebBaseUrl "http://localhost:$webPort" -ReadyExpectedStatus $ReadyExpectedStatus
if ($LASTEXITCODE -ne 0) { throw 'Docker Hub runtime smoke failed.' }
Write-Host 'Docker Hub runtime validation passed.'