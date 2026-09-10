param(
    [string]$BaseUrl = 'http://localhost:8088',
    [string]$WebUrl = 'http://localhost:4208',
    [int]$ReadyExpectedStatus = 200,
    [int]$RetryCount = 12,
    [int]$RetryDelaySeconds = 5
)

$ErrorActionPreference = 'Stop'

function Invoke-RequiredGet([string]$Uri, [int[]]$ExpectedStatus = @(200)) {
    $lastError = $null
    for ($attempt = 1; $attempt -le $RetryCount; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Uri -Method Get -UseBasicParsing -TimeoutSec 10
            $status = [int]$response.StatusCode
            if ($ExpectedStatus -notcontains $status) { throw "Unexpected HTTP $status for $Uri" }
            Write-Host "PASS $Uri -> $status"
            return $response
        } catch {
            $lastError = $_
            if ($null -ne $_.Exception.Response) {
                $status = [int]$_.Exception.Response.StatusCode
                if ($ExpectedStatus -contains $status) {
                    Write-Host "PASS $Uri -> $status"
                    return $null
                }
            }
            if ($attempt -lt $RetryCount) {
                Write-Host "WAIT $Uri attempt $attempt/$RetryCount"
                Start-Sleep -Seconds $RetryDelaySeconds
            }
        }
    }
    throw $lastError
}

Invoke-RequiredGet "$BaseUrl/"
Invoke-RequiredGet "$BaseUrl/health/live"
Invoke-RequiredGet "$BaseUrl/health/ready" @($ReadyExpectedStatus)
Invoke-RequiredGet "$BaseUrl/api/session" @(401)
Invoke-RequiredGet "$WebUrl/"

Write-Host "AppCondominio smoke tests passed. readiness=$ReadyExpectedStatus"
