# Resident Portal

The Resident Portal is a self-service surface of Conjunto al Día protected by `appcondominio.resident.access`.

## Security boundary

Resident context is never selected from client-supplied CommunityId or PersonId. The API resolves `ICurrentIdentity.UserId` from the authenticated PortalCorporativo JWT and maps it to active `People.AccessGrant` records by `ExternalUserId`.

Every unit-scoped operation calls `ResidentAccessService.RequireUnitAsync`, so a resident cannot access another unit by changing the route `unitId`.

## Resident capabilities

- View the resident's active units with property code, location and registered main area.
- View billing statement and receivables for an authorized unit.
- List active reservable areas in the resident's community.
- Request a reservation after validating that the area belongs to the same community.
- Authorize visitors using the authenticated resident's PersonId and CommunityId.

## Portal registration

Portal security assets register resource `appcondominio.resident` and permission `appcondominio.resident.access`. The Portal menu asset exposes `/resident` only through this permission.

## Validation

CI `34277123508` validates Gitleaks, production readiness, Angular 21 build and npm vulnerability scan, .NET build and NuGet vulnerability scan, unit tests, architecture tests, SQL Server/Testcontainers integration including ExternalUserId access lookup, Portal provisioning assets, and Docker image build.
