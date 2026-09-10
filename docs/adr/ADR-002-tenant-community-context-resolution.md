# ADR-002 - Tenant and Community Context Resolution

## Status

Accepted as a blocking architecture decision for Sprint 2 story S02-02.

## Context

AppCondominio requires tenant and community context to be treated as backend security boundaries. The current PortalCorporativo Security API provides JWT validation and user administration, and its `UserResponse` contract includes `TenantId`.

However, the current Security API exposes `GET /api/security/users/{id}` only inside the `/api/security` route group protected by the administrative permission `portal.security.manage`.

PortalCorporativo does not currently expose a self-service `/me`/current-user contract, a canonical signed `TenantId` claim, or a service-to-service identity contract suitable for AppCondominio to resolve the authenticated user's tenant without elevating the user's permissions.

PortalCorporativo also does not own AppCondominio community membership. `CommunityId` is a domain authorization scope and cannot be trusted from a browser-supplied header, query string, route value or arbitrary JWT claim invented by AppCondominio.

## Decision

S02-02 is BLOCKED for tenant/community resolution until the required contracts exist.

AppCondominio will NOT:

- grant `portal.security.manage` to ordinary application users;
- call the administrative user endpoint using an ordinary resident/administrator token as a workaround;
- invent `tenantId`, `communityId` or similar JWT claim names;
- trust `X-Tenant-Id`, `X-Community-Id`, query parameters or route values as authoritative security context;
- duplicate PortalCorporativo identity/user/tenant storage;
- infer tenant or community solely from UI state.

AppCondominio WILL:

- continue using the validated Portal-compatible JWT identity and signed `permission` claims implemented in S02-01;
- require PortalCorporativo to provide one of the approved tenant-resolution contracts below;
- resolve community context from AppCondominio-owned membership/authorization data once the Communities/People/Ownership model exists;
- validate every selected community against the authenticated user and resolved tenant on the server;
- fail closed when tenant/community context cannot be proven.

## Approved future tenant-resolution options

Preferred order:

1. Portal JWT contains a documented, signed canonical tenant claim.
2. Portal Security exposes an authenticated self endpoint such as `/api/security/me` returning the current user's `TenantId` without requiring administrative permission.
3. Portal provides a documented service-to-service contract so AppCondominio can resolve user metadata using its own workload identity rather than the end-user token.

Any option must be implemented in PortalCorporativo first and documented as a reusable contract.

## Community-context model

Community context belongs to AppCondominio. A future request may carry a community selector, but the value is only a requested scope. The backend must verify an active membership/relationship for the authenticated user inside the resolved tenant before establishing `CurrentCommunityId`.

Expected flow:

```text
Validated JWT
  -> authenticated UserId
  -> trusted TenantId resolution
  -> requested CommunityId
  -> AppCondominio membership/authorization check
  -> established request Tenant/Community context
```

## Consequences

- S02-02 cannot be marked implemented until Portal supplies a safe tenant-resolution contract and AppCondominio has a community-membership source.
- S02-03 permission registration can continue independently because permission resources/codes are an explicit Portal extension.
- Later domain modules must not accept tenant/community identifiers as trusted persistence filters until this ADR is satisfied.

## Security rationale

Tenant and community identifiers determine data visibility and write boundaries. Treating client-provided identifiers as authoritative would permit horizontal privilege escalation and cross-tenant data access. Blocking implementation is safer than introducing a temporary contract that later becomes an implicit production dependency.
