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
| 7 | Content/File integration | S02-07 | ADAPT | BLOCKED |
| 8 | Catalog integration | S02-08 | EXTEND/ADAPT | BLOCKED |
| 9 | Configuration integration | S02-09 | EXTEND/ADAPT | BLOCKED |
| 10 | Distributed health and resilience | S02-10 | ADAPT | NEXT |

## S02-01 validation
Branch `feature/s02-s02-01-portal-jwt-validation`; PR `#2`; CI `34161895440`. JWT validation matches Portal issuer/audience/signature/lifetime behavior and exposes signed `permission` claims. Portal production token issuance/OAuth/OIDC remains unavailable.

## S02-02 blocker
PR `#3`; ADR `docs/adr/ADR-002-tenant-community-context-resolution.md`. Portal `UserResponse` contains TenantId, but the available user endpoint requires administrative `portal.security.manage`. Safe tenant resolution requires a Portal self/service contract; CommunityId requires AppCondominio membership data.

## S02-03 validation
Branch `feature/s02-s02-03-portal-permission-authorization`; PR `#4`; CI `34162705886`. Shared Organizations read/manage permission contracts, Portal Security registration/provisioning and authorization tests are validated.

## S02-04 validation
Branch `feature/s02-s02-04-portal-menu-registration`; PR `#5`; CI `34163039648`. Portal module/menu registration and provisioning assets are validated.

## S02-05 blocker
Branch `feature/s02-s02-05-portal-audit-integration`; PR `#6`; ADR `docs/adr/ADR-003-audit-ingestion-service-identity.md`. Trusted service identity/actor validation is required before Portal Audit ingestion.

## S02-06 blocker
Branch `feature/s02-s02-06-portal-notification-integration`; PR `#7`; ADR `docs/adr/ADR-004-notification-service-identity-and-tenant.md`. Trusted service identity and tenant-aware Notification semantics are required.

## S02-07 blocker
Branch `feature/s02-s02-07-portal-content-integration`; PR `#8`; ADR `docs/adr/ADR-005-content-file-api-contract-required.md`. Portal Content currently has only placeholder contracts and a bootstrap endpoint.

## S02-08 blocker
Branch `feature/s02-s02-08-portal-catalog-integration`; PR `#9`; ADR `docs/adr/ADR-006-catalog-api-contract-required.md`. Portal Catalog currently has only placeholder contracts and a bootstrap endpoint.

## S02-09 blocker
Branch `feature/s02-s02-09-portal-configuration-integration`; ADR `docs/adr/ADR-007-configuration-tenant-resolution-required.md`.

Portal Configuration has functional Global/Tenant/Module/User contracts, but its current service hard-codes tenant `default` on create, resolve and scope lookup. Because Conjunto al Día is multi-tenant/white-label, AppCondominio will not treat `default` as a production substitute, invent a tenant header/query convention or duplicate the configuration engine. S02-09 remains BLOCKED until Portal derives TenantId from a trusted authenticated context while preserving its existing scope precedence model.

## Execution rule
Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
