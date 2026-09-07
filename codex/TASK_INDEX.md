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

Passed gates:

- Gitleaks secret scan.
- .NET 10 restore.
- NuGet direct/transitive vulnerability scan.
- .NET 10 Release build.
- Unit tests.
- Architecture tests.
- SQL Server/Testcontainers integration tests and EF migration execution.
- npm high/critical vulnerability scan.
- Angular 21 LTS production build.
- Docker builds for API, Worker and Web.

Security decision: Angular 18 alignment with the current PortalCorporativo frontend was rejected after CI found 58 vulnerabilities (32 high, 1 critical). AppCondominio uses Angular 21 LTS while preserving Portal integration through APIs/contracts/configuration. See `docs/adr/ADR-001-angular-security-baseline.md`.

## Sprint 02 - PortalCorporativo integration

| Order | Task | Story | Classification | Status |
|---:|---|---|---|---|
| 1 | Portal-compatible JWT validation and authenticated identity context | S02-01 | REUSE/ADAPT | VALIDATED |
| 2 | User/Tenant/Community context | S02-02 | ADAPT/CREATE | BLOCKED |
| 3 | Permission authorization | S02-03 | REUSE/EXTEND | NEXT |
| 4 | Menu registration | S02-04 | EXTEND | PLANNED |
| 5 | Audit integration | S02-05 | ADAPT | PLANNED |
| 6 | Notification integration | S02-06 | ADAPT | PLANNED |
| 7 | Content/File integration | S02-07 | ADAPT | PLANNED |
| 8 | Catalog integration | S02-08 | EXTEND/ADAPT | PLANNED |
| 9 | Configuration integration | S02-09 | EXTEND/ADAPT | PLANNED |
| 10 | Distributed health and resilience | S02-10 | ADAPT | PLANNED |

## S02-01 validation evidence

Branch: `feature/s02-s02-01-portal-jwt-validation`.
Base: Sprint 01 validated head `b6175c73a92cbe4ff81e16d27b680881323ec424`.
Implementation commit: `899258329925327f23d27090a2a0d312907b2a30`.
Validated CI run: `34161895440`.
PR: `#2` stacked against `foundation/sprint-01-architecture`.

Validated behavior:

- JWT bearer validation matches the current PortalCorporativo Security API contract.
- Canonical issuer: `portal-corporativo`.
- Canonical audience: `portal-corporativo-clients`.
- Signing secret is supplied externally and is not committed.
- Lifetime validation and one-minute clock skew are enabled.
- Signed `permission` claims are exposed through the local current-identity adapter.
- `/api/session` requires authentication and exposes the resolved local identity view.
- Organizations endpoints and technical messaging endpoint require authentication.
- Gitleaks, NuGet vulnerability scan, build, unit tests, architecture tests, SQL integration tests, npm audit, Angular build and Docker build all passed.

Portal limitation: PortalCorporativo Security API currently validates JWTs but does not provide production login/token issuance/OAuth/OIDC. AppCondominio does not duplicate or invent an IdP; production login remains BLOCKED until Portal supplies that capability.

## S02-02 blocking evidence

PortalCorporativo `UserResponse` includes `TenantId`, but the current endpoint that exposes it (`GET /api/security/users/{id}`) is inside the `/api/security` group protected by `portal.security.manage`. That administrative permission is not an acceptable dependency for ordinary AppCondominio users.

Portal currently provides no documented self-service current-user endpoint, canonical signed tenant claim, or workload/service-to-service contract for safe tenant lookup. Community membership is AppCondominio-owned and is not yet available in the domain model.

Therefore:

- authenticated UserId and permissions continue to come from the validated JWT implemented by S02-01;
- TenantId resolution is BLOCKED until Portal provides a safe reusable contract;
- CommunityId resolution is BLOCKED until an authenticated tenant exists and AppCondominio has community-membership/relationship data;
- no temporary tenant/community claim names or trusted client headers will be introduced;
- ordinary users will never receive `portal.security.manage` merely to resolve tenant context.

Decision record: `docs/adr/ADR-002-tenant-community-context-resolution.md`.

S02-03 may proceed independently by extending Portal Security with AppCondominio protected resources and permission definitions.

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
