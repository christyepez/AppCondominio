# AppCondominio - Implementation Strategy

## Delivery model

The product is implemented in 18 two-week sprints. Every sprint delivers a deployable, tested increment and ends with a PR into `main` after validation.

## Technology baseline

Backend:

- .NET 10 / ASP.NET Core 10
- `net10.0` for APIs, workers, libraries and tests
- EF Core 10 stable servicing line

Frontend:

- Angular 21 LTS (`21.2.x`)
- TypeScript `5.9.x`
- Node 20.19+ compatible build environment

Transactional persistence is SQL Server; Redis and RabbitMQ provide distributed cache and asynchronous messaging. Serilog/Seq and OpenTelemetry provide the initial observability foundation.

### Portal frontend compatibility decision

PortalCorporativo currently uses Angular 18. AppCondominio initially aligned to that version, but Sprint 1 `npm audit` found 58 vulnerabilities, including 32 high and 1 critical, with advisories affecting Angular runtime as well as build tooling. AppCondominio therefore moved to Angular 21 LTS.

This does not change the Portal-first strategy. AppCondominio reuses Portal capabilities through APIs, contracts, routes, menus, configuration and UX conventions. It MUST NOT downgrade its frontend framework merely to match the Portal runtime. See `docs/adr/ADR-001-angular-security-baseline.md`.

## Architecture strategy

1. Start as a Modular Monolith to keep deployment and transactional consistency simple.
2. Preserve bounded-context independence so modules can be extracted later.
3. Use PortalCorporativo for transverse capabilities rather than duplicating them.
4. Use CodexCommonAgents as the authoritative source for common implementation rules.
5. Treat tenant isolation, auditability and financial reversibility as architecture requirements from Sprint 1.
6. Apply `CodexCommonAgents/agents/03-backend-agent.md` to every backend implementation story.

## Common agent strategy

AppCondominio does not copy common agents. It pins a known compatible revision in `codex/COMMON_AGENTS.lock`.

For each task, the Coordinator Agent classifies the change as REUSE / EXTEND / ADAPT / CREATE / BLOCKED, assigns the responsible common/domain agent, validates architecture/security/tenant isolation and records the outcome in `codex/TASK_INDEX.md`.

## Branch strategy

- `main`: stable integration/release line.
- `foundation/*`: initial platform work.
- `feature/sXX-STORY-description`: functional stories.
- `fix/sXX-STORY-description`: defects.
- `chore/*`: controlled maintenance.
- `release/x.y.z`: release hardening when needed.
- `hotfix/x.y.z`: production critical fixes.

Prefer squash merge so each story produces one traceable main-line commit.

## PR gates

A PR cannot merge until applicable gates pass:

- .NET 10 restore and Release build
- NuGet direct/transitive vulnerability scan
- backend unit tests
- architecture tests
- SQL Server integration tests
- npm high/critical vulnerability scan
- Angular production build
- Docker image build using .NET 10 images
- Gitleaks secret scan
- tenant-isolation tests for tenant-owned features
- migration validation for database changes
- documentation/task-index update

GitHub Dependency Review may be enabled later when the repository Dependency Graph is available; it is not relied upon as the sole vulnerability gate.

## Environment strategy

Local uses Docker Compose. DEV deploys after validated merge when CI is enabled. TEST/UAT promotes immutable images without rebuilding. PROD requires explicit approval, migration precheck, backup/restore and rollback runbook. Configuration is externalized and images remain identical across environments.

## Docker topology

Initial services:

- appcondominio-web (Angular 21 + Nginx)
- appcondominio-api (.NET 10)
- appcondominio-worker (.NET 10)
- sqlserver
- redis
- rabbitmq
- seq

PortalCorporativo services are consumed through configured endpoints or a shared compose/network in integrated development environments. AppCondominio must not fork Portal services into this repository.

## Data strategy

- SQL Server is the primary transactional store.
- Modules use independent schemas and DbContexts where practical.
- No module directly updates another module's tables.
- Outbox/Inbox provide reliable asynchronous integration.
- Financial and legal records are corrected through reversal/versioning rather than physical deletion.
- Migrations are versioned with source code and validated in CI.

## Multi-tenant strategy

Initial default: shared application/database infrastructure with tenant-owned rows explicitly scoped by TenantId and CommunityId where applicable. Architecture must allow selected tenants to move to dedicated databases later without changing business use cases.

Required protections include authenticated tenant context, server-side authorization, query/write scoping, explicit cross-tenant administration paths, automated isolation tests, tenant-aware cache keys and tenant-aware events/jobs.

## Integration strategy

PortalCorporativo capabilities are integrated through local application ports/adapters. Domain code never references remote HTTP details. Prefer official shared contracts/packages when PortalCorporativo provides them.

## Sprint roadmap

1. Foundation: repository, .NET 10/Angular 21 skeleton, SQL Server, Docker, Redis, RabbitMQ, Worker, observability, CI and architecture tests.
2. Portal integration: Security, tenant/user context, permissions, menu, audit, notifications, content, catalogs and configuration.
3. SaaS: organizations, plans, subscriptions, white-label and module limits.
4. Communities and physical structure.
5. Properties and allocation coefficients.
6. People and ownership.
7. Billing engine.
8. Monthly generation.
9. Collections.
10. Banking and SRI.
11. Accounting.
12. Treasury.
13. Budget and reporting.
14. Procurement.
15. Maintenance.
16. Reservations and security operations.
17. Governance.
18. Production readiness.

## Sprint 1 validated baseline

GitHub Actions run `34161344428` validated the final Sprint 1 baseline with:

- Gitleaks: PASS
- .NET restore: PASS
- NuGet vulnerability scan: PASS
- .NET 10 Release build: PASS
- unit tests: PASS
- architecture tests: PASS
- SQL Server/Testcontainers integration tests: PASS
- npm vulnerability scan: PASS
- Angular 21 production build: PASS
- Docker build for API/Worker/Web: PASS

## First implementation sequence

1. S01-01 repository standards and .NET 10 solution skeleton.
2. S01-02 backend host + modular bootstrapper.
3. S01-03 reference Organizations module.
4. S01-04 SQL Server + EF Core 10 + migrations.
5. S01-05 Angular shell.
6. S01-06 Docker Compose with .NET 10 images.
7. S01-07 RabbitMQ/Redis/Worker.
8. S01-08 observability/error handling.
9. S01-09 automated test foundation.
10. S01-10 GitHub Actions and security gates.
