param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AdminToken,

    [string]$ManifestPath = "infrastructure/portal/security/appcondominio-security-registration.json"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ManifestPath)) {
    throw "Security registration manifest not found: $ManifestPath"
}

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
$headers = @{ Authorization = "Bearer $AdminToken" }
$base = $BaseUrl.TrimEnd('/')

function Invoke-PortalRegistration {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] $Payload
    )

    $request = @{
        Uri = "$base$Path"
        Method = "Post"
        Headers = $headers
        ContentType = "application/json"
        Body = ($Payload | ConvertTo-Json -Depth 8)
        SkipHttpErrorCheck = $true
    }

    $response = Invoke-WebRequest @request

    if (($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) -or $response.StatusCode -eq 409) {
        return
    }

    throw "Portal Security registration failed for $Path with HTTP $($response.StatusCode): $($response.Content)"
}

Invoke-PortalRegistration -Path "/api/security/resources" -Payload $manifest.resource

foreach ($permission in $manifest.permissions) {
    Invoke-PortalRegistration -Path "/api/security/permissions" -Payload $permission
}

Write-Host "AppCondominio Portal Security registration completed. HTTP 409 responses were treated as already registered."
