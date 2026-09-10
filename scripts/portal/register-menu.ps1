param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$AdminToken,

    [string]$ManifestPath = "infrastructure/portal/menu/appcondominio-menu-registration.json"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ManifestPath)) {
    throw "Menu registration manifest not found: $ManifestPath"
}

$manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
$headers = @{ Authorization = "Bearer $AdminToken" }
$base = $BaseUrl.TrimEnd('/')
$moduleCode = $manifest.module.moduleCode
$menuId = $null
$existingItems = @()

function Invoke-PortalRequest {
    param(
        [Parameter(Mandatory = $true)] [string]$Method,
        [Parameter(Mandatory = $true)] [string]$Path,
        $Payload = $null
    )

    $request = @{
        Uri = "$base$Path"
        Method = $Method
        Headers = $headers
        SkipHttpErrorCheck = $true
    }

    if ($null -ne $Payload) {
        $request.ContentType = "application/json"
        $request.Body = ($Payload | ConvertTo-Json -Depth 12)
    }

    return Invoke-WebRequest @request
}

# Read current module state first. Portal currently returns menu items, not a menu-definition DTO.
$lookup = Invoke-PortalRequest -Method "Get" -Path "/api/menu/modules/$moduleCode"

if ($lookup.StatusCode -eq 200) {
    $body = $lookup.Content | ConvertFrom-Json
    $existingItems = @($body.data)

    if ($existingItems.Count -gt 0) {
        $menuId = $existingItems[0].menuId
    }
    else {
        throw "Portal Menu module '$moduleCode' exists but contains no items. The current Portal API does not expose its MenuId through the module lookup, so safe idempotent provisioning cannot continue. Add a Portal menu-definition lookup contract instead of creating a duplicate menu."
    }
}
elseif ($lookup.StatusCode -eq 404) {
    $createMenu = Invoke-PortalRequest -Method "Post" -Path "/api/menu" -Payload $manifest.module

    if ($createMenu.StatusCode -lt 200 -or $createMenu.StatusCode -ge 300) {
        throw "Portal Menu creation failed with HTTP $($createMenu.StatusCode): $($createMenu.Content)"
    }

    $created = $createMenu.Content | ConvertFrom-Json
    $menuId = $created.data

    if ([string]::IsNullOrWhiteSpace([string]$menuId)) {
        throw "Portal Menu creation succeeded but did not return a menu identifier."
    }
}
else {
    throw "Portal Menu lookup failed with HTTP $($lookup.StatusCode): $($lookup.Content)"
}

foreach ($item in $manifest.items) {
    $alreadyExists = $existingItems | Where-Object { $_.code -eq $item.code } | Select-Object -First 1
    if ($null -ne $alreadyExists) {
        Write-Host "Menu item '$($item.code)' already exists; skipping."
        continue
    }

    $payload = [ordered]@{
        menuId = $menuId
        parentId = $item.parentId
        code = $item.code
        label = $item.label
        route = $item.route
        icon = $item.icon
        order = $item.order
        resourceKey = $item.resourceKey
        permissionCode = $item.permissionCode
        metadataJson = $item.metadataJson
    }

    $createItem = Invoke-PortalRequest -Method "Post" -Path "/api/menu/items" -Payload $payload
    if ($createItem.StatusCode -lt 200 -or $createItem.StatusCode -ge 300) {
        throw "Portal Menu item '$($item.code)' creation failed with HTTP $($createItem.StatusCode): $($createItem.Content)"
    }

    Write-Host "Registered menu item '$($item.code)'."
}

Write-Host "AppCondominio Portal Menu registration completed for module '$moduleCode'."
