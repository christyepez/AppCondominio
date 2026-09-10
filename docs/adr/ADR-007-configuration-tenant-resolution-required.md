# ADR-007 - Tenant-Aware Portal Configuration Resolution Is Required

## Status

Accepted as a blocking architecture decision for Sprint 2 story S02-09.

## Context

PortalCorporativo Configuration API has functional contracts for creating/updating configuration and resolving effective values across the scopes Global, Tenant, Module and User.

However, the current `ConfigurationService` hard-codes tenant `default` for creation, effective resolution and scope queries. The request contracts do not carry a tenant identifier and the service does not derive tenant from authenticated context.

AppCondominio / Conjunto al Día is a multi-tenant white-label SaaS. Tenant-level visual/functional configuration is therefore a product requirement, not an optional future optimization.

## Decision

S02-09 is `BLOCKED` for production integration until PortalCorporativo resolves tenant context from a trusted authenticated source and removes the hard-coded `default` assumption from Configuration operations.

AppCondominio will NOT:

- treat tenant `default` as a production substitute for condominium tenants;
- invent a tenant query/header convention for Portal Configuration;
- grant client-controlled tenant identifiers authority over effective configuration;
- duplicate Portal's generic configuration engine locally;
- create speculative configuration keys with no current product use case merely to exercise the API.

## Required Portal enhancement

Portal Configuration should preserve its current effective resolution model while obtaining tenant from a trusted context. The contract should clearly define:

1. canonical authenticated TenantId resolution;
2. global vs tenant vs module vs user precedence;
3. authorization for configuration read/manage;
4. whether service/workload identity is required for domain-side reads/provisioning;
5. cache invalidation/version semantics for consumers;
6. behavior for missing tenant context and cross-tenant administration;
7. audit/correlation semantics for changes.

## Current reusable behavior retained

The existing scope model is compatible with AppCondominio conceptually:

- Global = 0
- Tenant = 1
- Module = 2
- User = 3

and effective resolution orders by more specific scope and then version. This can be reused after tenant context becomes trustworthy.

## Consequences

- S02-09 remains BLOCKED rather than introducing a white-label implementation tied to `default`.
- AppCondominio will not create a competing configuration store.
- S02-10 distributed health/resilience can proceed independently.
