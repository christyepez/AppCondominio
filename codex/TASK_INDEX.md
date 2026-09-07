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
| 3 | Permission authorization | S02-03 | REUSE/EXTEND | VALIDATED |
| 4 | Menu registration | S02-04 | EXTEND | IMPLEMENTED / VALIDATION RUNNING |
| 5 | Audit integration | S02-05 | ADAPT | NEXT |
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

Portal limitation: PortalCorporativo Security API currently validates JWTs but does not provide production login/token issuance/OAuth/OIDC. AppCondominio does not duplicate or invent an IdP; production login remains BLOCKED until Portal supplies that capability.

## S02-02 blocker

S02-02 is documented in PR `#3`. Portal `UserResponse` includes TenantId, but the available user endpoint requires administrative `portal.security.manage`. AppCondominio will not elevate ordinary users or trust client-supplied tenant/community identifiers. Safe tenant resolution remains blocked pending a Portal self/service contract; community context remains blocked pending AppCondominio membership data.

## S02-03 validation

Branch: `feature/s02-s02-03-portal-permission-authorization`.
Base: S02-01 validated documentation head `ea0c5d3a1d963e1e4ace946f1b64dfca1ec93558`.
PR: `#4` stacked against S02-01.
Validated code run: `34162705886`.

Passed Gitleaks, NuGet scan, .NET 10 build, unit tests, architecture tests, integration tests, npm audit, Angular build and Docker builds.

Implemented:

- shared permission contract in `AppCondominio.Contracts`;
- `appcondominio.organizations.read` and `appcondominio.organizations.manage`;
- endpoint-level authorization policies using signed Portal `permission` claims;
- Portal Security registration manifest and PowerShell 7 provisioning script;
- authorization tests separating read and manage capabilities.

## S02-04 implementation

Branch: `feature/s02-s02-04-portal-menu-registration`.
Base: S02-03 head `63ef52c0a0ce9009229ce84da6c6cb67e19e82b4`.

Implemented against the current Portal Menu contracts:

- module code `appcondominio`, name `Conjunto al Día`;
- Organizations navigation item pointing to the existing Angular `/organizations` route;
- item resource `appcondominio.organizations` and permission `appcondominio.organizations.read` from S02-03;
- Portal Menu registration manifest;
- PowerShell 7 provisioning script using `GET /api/menu/modules/{moduleCode}`, `POST /api/menu` and `POST /api/menu/items`;
- no speculative menu items for bounded contexts whose routes do not yet exist;
- fail-closed behavior when an existing empty Portal menu cannot expose its MenuId through the current API contract;
- CI validation for Portal PowerShell script syntax and registration JSON manifests.

Current Portal limitation: module lookup returns menu items rather than a menu-definition DTO. Fully idempotent recovery of an existing empty menu requires a future Portal contract exposing MenuId for the module.

Documentation: `docs/menu/portal-menu-registration.md`.

Final validation remains pending the latest GitHub Actions run.

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
