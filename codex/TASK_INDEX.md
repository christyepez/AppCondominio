# AppCondominio - Codex Task Index

## Sprint 01 - Architecture and foundation

| Order | Task | Story | Classification | Responsible agent | Status | Depends on |
|---:|---|---|---|---|---|---|
| 1 | Repository standards and .NET 10 solution skeleton | S01-01 | CREATE | Coordinator + Solution Architect | VALIDATED | - |
| 2 | Backend host and modular bootstrapper (.NET 10) | S01-02 | CREATE | Solution Architect + Backend Agent | VALIDATED | S01-01 |
| 3 | Organizations reference module | S01-03 | CREATE | Domain + Backend Agent | VALIDATED | S01-02 |
| 4 | SQL Server + EF Core 10 persistence and migrations | S01-04 | CREATE | Data + Backend Agent | VALIDATED | S01-03 |
| 5 | Angular 21 LTS shell integration skeleton | S01-05 | REUSE/EXTEND | Portal Reuse + Frontend Agent | VALIDATED | S01-01 |
| 6 | Docker Compose development topology | S01-06 | CREATE/REUSE | DevOps Agent | VALIDATED | S01-02,S01-05 |
| 7 | Redis, RabbitMQ and Worker foundation | S01-07 | EXTEND/CREATE | Portal Reuse + Backend + DevOps | VALIDATED | S01-02,S01-06 |
| 8 | Error handling, logging and observability | S01-08 | EXTEND/ADAPT | Architecture + Security/Observability | VALIDATED | S01-02,S01-06 |
| 9 | Automated test foundation | S01-09 | CREATE | QA Agent | VALIDATED | S01-03,S01-04 |
| 10 | GitHub Actions and security gates | S01-10 | CREATE | DevOps + Security + QA | VALIDATED | S01-06,S01-09 |

## Sprint 01 validation evidence

Final validated CI run: `34161344428`.

Passed gates: Gitleaks, .NET 10 restore/build, NuGet vulnerability scan, unit tests, architecture tests, SQL Server/Testcontainers integration and EF migrations, npm vulnerability scan, Angular 21 production build and Docker builds.

Security decision: Angular 18 alignment with the current PortalCorporativo frontend was rejected after CI found 58 vulnerabilities (32 high, 1 critical). AppCondominio uses Angular 21 LTS while preserving Portal integration through APIs/contracts/configuration. See `docs/adr/ADR-001-angular-security-baseline.md`.

## Sprint 02 - PortalCorporativo integration

| Order | Task | Story | Classification | Status |
|---:|---|---|---|---|
| 1 | Portal-compatible JWT validation and authenticated identity context | S02-01 | REUSE/ADAPT | VALIDATED |
| 2 | User/Tenant/Community context | S02-02 | ADAPT/CREATE | BLOCKED |
| 3 | Permission authorization | S02-03 | REUSE/EXTEND | VALIDATED |
| 4 | Menu registration | S02-04 | EXTEND | IMPLEMENTED / VALIDATION RUNNING |
| 5 | Audit integration | S02-05 | ADAPT | BLOCKED |
| 6 | Notification integration | S02-06 | ADAPT | NEXT |
| 7 | Content/File integration | S02-07 | ADAPT | PLANNED |
| 8 | Catalog integration | S02-08 | EXTEND/ADAPT | PLANNED |
| 9 | Configuration integration | S02-09 | EXTEND/ADAPT | PLANNED |
| 10 | Distributed health and resilience | S02-10 | ADAPT | PLANNED |

## S02-01 validation evidence

Branch: `feature/s02-s02-01-portal-jwt-validation`.
Validated CI run: `34161895440`.
PR: `#2` stacked against Sprint 01.

JWT validation matches Portal issuer/audience/signature/lifetime behavior and exposes signed `permission` claims through the local identity adapter. Portal production token issuance/OAuth/OIDC remains unavailable; AppCondominio does not duplicate an IdP.

## S02-02 blocker

PR `#3`; ADR `docs/adr/ADR-002-tenant-community-context-resolution.md`.

Portal `UserResponse` contains TenantId, but the current user endpoint requires administrative `portal.security.manage`. AppCondominio will not elevate ordinary users or trust client-supplied tenant/community identifiers. Safe tenant resolution requires a Portal self/service contract; CommunityId requires AppCondominio membership data.

## S02-03 validation

Branch: `feature/s02-s02-03-portal-permission-authorization`.
PR: `#4` stacked against S02-01.
Validated code run: `34162705886`.

Implemented shared permission contracts, Organizations read/manage policies, Portal Security registration manifest/provisioning and authorization tests. All normal CI gates including Docker passed.

## S02-04 implementation

Branch: `feature/s02-s02-04-portal-menu-registration`.
Base: S02-03 head `63ef52c0a0ce9009229ce84da6c6cb67e19e82b4`.
CI run in validation: `34163039648`.

Implemented:

- Portal module `appcondominio` / `Conjunto al Día`;
- Organizations item -> `/organizations`;
- S02-03 resource/permission linkage;
- Menu registration manifest and PowerShell 7 provisioning;
- fail-closed behavior for the current Portal empty-menu/MenuId contract gap;
- CI syntax validation of Portal provisioning scripts and JSON manifests.

Documentation: `docs/menu/portal-menu-registration.md`.

## S02-05 blocker

Branch: `feature/s02-s02-05-portal-audit-integration`.
Decision: `docs/adr/ADR-003-audit-ingestion-service-identity.md`.

The current Portal Audit write contract requires `portal.audit.write` but trusts `ActorId` from the request body. Portal does not currently provide a documented workload/service-to-service identity for AppCondominio. Forwarding an ordinary end-user JWT would either require granting a transverse audit-write permission to users or permit forged actor metadata if the Audit API is directly reachable.

Therefore AppCondominio will not implement an unsafe adapter, use a static bearer token, write directly to PortalAudit, or duplicate the audit engine. S02-05 remains BLOCKED until Portal provides trusted audit-ingestion identity/actor validation. Critical future audit delivery should additionally use a reliable outbox-based pattern rather than couple domain commits to a synchronous remote call.

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
