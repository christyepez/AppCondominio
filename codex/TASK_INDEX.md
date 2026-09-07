# AppCondominio - Codex Task Index

## Sprint 01 - Architecture and foundation

| Order | Task | Story | Classification | Status |
|---:|---|---|---|---|
| 1 | Repository standards and .NET 10 solution skeleton | S01-01 | CREATE | VALIDATED |
| 2 | Backend host and modular bootstrapper | S01-02 | CREATE | VALIDATED |
| 3 | Organizations reference module | S01-03 | CREATE | VALIDATED |
| 4 | SQL Server + EF Core 10 persistence and migrations | S01-04 | CREATE | VALIDATED |
| 5 | Angular 21 LTS shell | S01-05 | REUSE/EXTEND | VALIDATED |
| 6 | Docker Compose topology | S01-06 | CREATE/REUSE | VALIDATED |
| 7 | Redis, RabbitMQ and Worker | S01-07 | EXTEND/CREATE | VALIDATED |
| 8 | Error handling, logging and observability | S01-08 | EXTEND/ADAPT | VALIDATED |
| 9 | Automated test foundation | S01-09 | CREATE | VALIDATED |
| 10 | GitHub Actions and security gates | S01-10 | CREATE | VALIDATED |

Sprint 01 final validated CI run: `34161344428`.

## Sprint 02 - PortalCorporativo integration

| Order | Task | Story | Classification | Status |
|---:|---|---|---|---|
| 1 | Portal-compatible JWT validation and identity | S02-01 | REUSE/ADAPT | VALIDATED |
| 2 | User/Tenant/Community context | S02-02 | ADAPT/CREATE | BLOCKED |
| 3 | Permission authorization | S02-03 | REUSE/EXTEND | VALIDATED |
| 4 | Menu registration | S02-04 | EXTEND | VALIDATED |
| 5 | Audit integration | S02-05 | ADAPT | BLOCKED |
| 6 | Notification integration | S02-06 | ADAPT | BLOCKED |
| 7 | Content/File integration | S02-07 | ADAPT | NEXT |
| 8 | Catalog integration | S02-08 | EXTEND/ADAPT | PLANNED |
| 9 | Configuration integration | S02-09 | EXTEND/ADAPT | PLANNED |
| 10 | Distributed health and resilience | S02-10 | ADAPT | PLANNED |

## S02-01 validation

Branch `feature/s02-s02-01-portal-jwt-validation`; PR `#2`; CI `34161895440`.

JWT validation matches Portal issuer/audience/signature/lifetime behavior and exposes signed `permission` claims through the local identity adapter. Portal production token issuance/OAuth/OIDC remains unavailable; AppCondominio does not duplicate an IdP.

## S02-02 blocker

PR `#3`; ADR `docs/adr/ADR-002-tenant-community-context-resolution.md`.

Portal `UserResponse` contains TenantId, but the available user endpoint requires administrative `portal.security.manage`. AppCondominio will not elevate ordinary users or trust client-supplied tenant/community identifiers. Safe tenant resolution requires a Portal self/service contract; CommunityId requires AppCondominio membership data.

## S02-03 validation

Branch `feature/s02-s02-03-portal-permission-authorization`; PR `#4`; validated code run `34162705886`.

Implemented shared permission contracts, Organizations read/manage policies, Portal Security registration manifest/provisioning and authorization tests. All normal CI gates including Docker passed.

## S02-04 validation

Branch `feature/s02-s02-04-portal-menu-registration`; PR `#5`; validated CI run `34163039648`.

Implemented Portal module `appcondominio` / `Conjunto al Día`, Organizations navigation item to `/organizations`, permission linkage to S02-03, Menu registration manifest, PowerShell 7 provisioning, fail-closed handling for the existing-empty-menu/MenuId contract gap, and CI validation of Portal scripts/manifests.

## S02-05 blocker

Branch `feature/s02-s02-05-portal-audit-integration`; PR `#6`; ADR `docs/adr/ADR-003-audit-ingestion-service-identity.md`.

Portal Audit write requires `portal.audit.write` but trusts request-body `ActorId`. No documented AppCondominio workload/service identity exists. AppCondominio will not forward ordinary end-user tokens, store static bearer tokens, write directly to PortalAudit or duplicate the engine. Production audit ingestion requires trusted caller identity/actor validation and should later use reliable outbox-based delivery for critical events.

## S02-06 blocker

Branch `feature/s02-s02-06-portal-notification-integration`.
Decision: `docs/adr/ADR-004-notification-service-identity-and-tenant.md`.

Portal Notification supports templates, immediate/scheduled sending and idempotency, but runtime send/schedule require `portal.notification.send`, recipients are caller supplied, and the current service hard-codes `Tenant = "default"`. Portal has no documented workload/service identity or tenant-aware domain-notification ingestion contract.

AppCondominio will not grant ordinary users transverse notification-send authority, use end-user JWTs as general system credentials, assume production tenant `default`, store static bearer tokens, or duplicate Portal template/provider/retry engines. Templates will be added with concrete bounded-context use cases rather than speculatively.

S02-06 remains BLOCKED until Portal supplies trusted service identity and tenant-aware notification semantics.

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
