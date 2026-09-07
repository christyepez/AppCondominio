# AppCondominio - Implementation Strategy

## Delivery model

The product is implemented in 18 two-week sprints. Every sprint delivers a deployable, tested increment and ends with a PR into `main` after validation.

## Technology baseline

All backend components use .NET 10 / ASP.NET Core 10 and target `net10.0`. This includes APIs, workers, domain/application/infrastructure projects, tests, EF Core tooling, Docker runtime/build images and CI jobs. Package versions are centralized and use stable .NET 10 servicing releases.

Frontend remains Angular aligned with PortalCorporativo. Transactional persistence is SQL Server; Redis and RabbitMQ provide distributed cache and asynchronous messaging.

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

- .NET 10 SDK/runtime validation
- backend restore/build
- backend unit tests
- architecture tests
- integration tests
- Angular lint/test/build
- Docker image build using .NET 10 images
- secret scan
- dependency/vulnerability scan
- tenant-isolation tests for tenant-owned features
- migration validation for database changes
- documentation/task-index update

## Environment strategy

Local uses Docker Compose. DEV deploys after validated merge when CI is enabled. TEST/UAT promotes immutable images without rebuilding. PROD requires explicit approval, migration precheck, backup/restore and rollback runbook. Configuration is externalized and images remain identical across environments.

## Docker topology

Initial services:

- appcondominio-web
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

1. Foundation: repository, .NET 10/Angular skeleton, SQL Server, Docker, Redis, RabbitMQ, Worker, observability, CI and architecture tests.
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

## First implementation sequence

1. S01-01 repository standards and .NET 10 solution skeleton.
2. S01-02 backend host + modular bootstrapper.
3. S01-03 reference Organizations module.
4. S01-04 SQL Server + EF Core 10 + migrations.
5. S01-05 Angular shell integration skeleton.
6. S01-06 Docker Compose with .NET 10 images.
7. S01-07 RabbitMQ/Redis/Worker.
8. S01-08 observability/error handling.
9. S01-09 automated test foundation on .NET 10.
10. S01-10 GitHub Actions and security gates using .NET 10 SDK.
