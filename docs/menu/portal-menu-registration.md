# Portal Menu registration

## Purpose

AppCondominio reuses PortalCorporativo Menu API for module navigation. It does not create a parallel menu engine or persist a separate global navigation tree.

Sprint 2 story S02-04 registers only the application surface that exists today:

- Module code: `appcondominio`
- Module name: `Conjunto al Día`
- Item code: `appcondominio.organizations`
- Route: `/organizations`
- Resource: `appcondominio.organizations`
- Permission: `appcondominio.organizations.read`

Future bounded contexts are not pre-registered before their routes and permissions exist.

## Portal contracts used

The implementation follows the current Portal Menu contracts exactly:

- `CreateMenuRequest(string ModuleCode, string Name)`
- `CreateMenuItemRequest(Guid MenuId, Guid? ParentId, string Code, string Label, string Route, string? Icon, int Order, string ResourceKey, string PermissionCode, string? MetadataJson)`

Provisioning uses:

- `GET /api/menu/modules/{moduleCode}` with `portal.menu.read` to inspect current state;
- `POST /api/menu` with `portal.menu.manage` to create the module menu when absent;
- `POST /api/menu/items` with `portal.menu.manage` to create missing items.

## Provisioning

Manifest:

`infrastructure/portal/menu/appcondominio-menu-registration.json`

PowerShell 7:

```powershell
./scripts/portal/register-menu.ps1 `
  -BaseUrl "https://portal-menu.example" `
  -AdminToken $env:PORTAL_ADMIN_TOKEN
```

The token must be supplied externally and needs the Portal menu read/manage permissions required by the endpoints. No token is stored in the repository.

## Idempotency and current Portal limitation

The script first reads the module and skips an item whose `code` already exists. When the module does not exist, the script creates it and uses the returned menu identifier to register its items.

The current Portal endpoint `GET /api/menu/modules/{moduleCode}` returns only `MenuItemResponse[]`, not a menu-definition response. Therefore an already-existing **empty** menu does not expose its `MenuId`. In that state the script fails closed instead of creating a duplicate module menu.

A future Portal enhancement should expose a menu-definition/current-module DTO containing `MenuId`, `ModuleCode` and `Name`; after that contract exists, provisioning can become fully idempotent even for empty menus.

## Authorization behavior

Portal Menu derives the requested action from the last segment of `PermissionCode` and checks it against `ResourceKey`. For the Organizations item this results in:

- resource `appcondominio.organizations`
- action `read`

which matches the resource/permission registered in S02-03.

## Frontend alignment

The registered `/organizations` route already exists in the Angular shell. The Portal menu metadata therefore points to an implemented route and does not introduce speculative navigation.
