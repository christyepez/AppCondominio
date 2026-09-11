# ADR-006 - Portal Catalog Contract Is Required Before Domain Integration

## Status

Accepted as a blocking dependency decision for Sprint 2 story S02-08.

## Context

AppCondominio should reuse PortalCorporativo for global/shared catalogs and parametrizable reference data where that capability exists.

The current `Portal.Catalog.Contracts` project contains only the placeholder `Class1.cs`. The current Catalog API bootstraps Portal foundation and exposes only:

`GET /` -> `{ service = "Portal.Catalog.Api", status = "bootstrap" }`

No reusable catalog contracts or functional endpoints currently exist.

## Decision

S02-08 is `BLOCKED` until PortalCorporativo implements and documents the Catalog API.

AppCondominio will NOT:

- invent Portal catalog DTOs/endpoints;
- create a competing generic global catalog engine;
- access Portal catalog storage/database internals;
- move domain-owned enumerations/rules into a speculative shared catalog;
- treat client-supplied catalog codes as valid without server-side validation.

## Minimum reusable contract expected

Portal Catalog should define, as applicable:

1. catalog definition/code and description;
2. catalog items with stable code, label/value, order and active state;
3. global/tenant scope and resolution rules;
4. read/manage authorization;
5. create/update/activate/deactivate operations;
6. effective-item lookup by catalog code and tenant;
7. uniqueness/idempotency semantics;
8. metadata/localization support where needed;
9. correlation/audit behavior;
10. version/cache invalidation semantics for consumers.

## Ownership rule

Only cross-cutting/global reference data belongs in Portal Catalog. Condominium business concepts with lifecycle/invariants remain in their owning AppCondominio bounded context even after Portal Catalog becomes available.

## Consequences

- S02-08 remains blocked without creating a parallel generic catalog service.
- S02-09 Configuration integration may proceed independently using the existing Portal Configuration contracts.
