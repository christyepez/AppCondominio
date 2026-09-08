param(
    [string]$BaseUrl = 'http://localhost:8088',
    [string]$WebUrl = 'http://localhost:4208'
)

$ErrorActionPreference = 'Stop'

function Invoke-RequiredGet([string]$Uri, [int[]]$ExpectedStatus = @(200)) {
    try {
        $response = Invoke-WebRequest -Uri $Uri -Method Get -UseBasicParsing -TimeoutSec 10
        if ($ExpectedStatus -notcontains [int]$response.StatusCode) {
            throw "Unexpected HTTP $($response.StatusCode) for $Uri"
        }
        Write-Host "PASS $Uri -> $($response.StatusCode)"
        return $response
    } catch {
        if ($null -ne $_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
            if ($ExpectedStatus -contains $status) {
                Write-Host "PASS $Uri -> $status"
                return $null
            }
        }
        throw
    }
}

Invoke-RequiredGet "$BaseUrl/"
Invoke-RequiredGet "$BaseUrl/health/live"
Invoke-RequiredGet "$BaseUrl/health/ready"
Invoke-RequiredGet "$BaseUrl/api/session" @(401)
Invoke-RequiredGet "$WebUrl/"

Write-Host 'AppCondominio smoke tests passed.'
