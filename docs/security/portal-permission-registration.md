# Portal Security permission registration

## Purpose

AppCondominio reuses PortalCorporativo Security API for global users, roles, permissions and authorization. AppCondominio does not create a parallel permission store.

Sprint 2 story S02-03 introduces the first domain resource and permission definitions:

- Resource: `appcondominio.organizations`
- Permission: `appcondominio.organizations.read`
- Permission: `appcondominio.organizations.manage`

## Runtime authorization

AppCondominio validates the Portal-compatible JWT and uses signed `permission` claims. The Organizations API requires:

- `GET /api/organizations/{id}` -> `appcondominio.organizations.read`
- `POST /api/organizations` -> `appcondominio.organizations.manage`

Authentication alone is not sufficient for these endpoints.

## Portal registration

The registration manifest is:

`infrastructure/portal/security/appcondominio-security-registration.json`

Provisioning uses the current Portal endpoints:

- `POST /api/security/resources`
- `POST /api/security/permissions`

Those Portal endpoints require `portal.security.manage`; therefore provisioning must run with an administrator/deployment token, never an ordinary resident or application-user token.

PowerShell 7 example:

```powershell
./scripts/portal/register-security.ps1 `
  -BaseUrl "https://portal-security.example" `
  -AdminToken $env:PORTAL_ADMIN_TOKEN
```

The token is supplied externally and must not be committed. HTTP 409 is treated as an already-registered resource/permission so the provisioning operation can be safely repeated for the same manifest.

## Role assignment

This story registers resource/permission definitions only. Assignment of these permissions to concrete Portal roles is environment/business-policy dependent and must be performed through Portal Security administration. AppCondominio does not mutate Portal role assignments implicitly at application startup.

## Future permissions

New bounded contexts extend the same shared contract and Portal registration manifest/process. Permission codes follow:

`appcondominio.<resource>.<action>`

Examples:

- `appcondominio.communities.read`
- `appcondominio.communities.manage`
- `appcondominio.billing.read`
- `appcondominio.billing.manage`

Permissions must be introduced only when the corresponding application surface exists; avoid speculative permission catalogs.
