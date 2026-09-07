# AppCondominio - Implementation Strategy

## Delivery model

The product is implemented in 18 two-week sprints. Every sprint delivers a deployable, tested increment and ends with a PR into `main` after validation.

## Architecture strategy

1. Start as a Modular Monolith to keep deployment and transactional consistency simple.
2. Preserve bounded-context independence so modules can be extracted later.
3. Use PortalCorporativo for transverse capabilities rather than duplicating them.
4. Use CodexCommonAgents as the authoritative source for common implementation rules.
5. Treat tenant isolation, auditability and financial reversibility as architecture requirements from Sprint 1.

## Common agent strategy

AppCondominio does not copy common agents. It pins a known compatible revision in `codex/COMMON_AGENTS.lock`.

For each task, the Coordinator Agent:

1. Reads local `AGENTS.md` and project context.
2. Loads the pinned common-agent revision.
3. Checks PortalCorporativo reuse.
4. Assigns the responsible common or local domain agent.
5. Classifies the change as REUSE / EXTEND / ADAPT / CREATE / BLOCKED.
6. Executes only the files needed for the story.
7. Validates tests, security, tenant isolation and documentation.
8. Records the result in `codex/TASK_INDEX.md`.

## Local domain agents

Common roles stay in CodexCommonAgents. AppCondominio may define only domain-specific extensions, for example:

- Condominium Domain Agent
- Billing and Collections Agent
- Accounting and Treasury Agent
- Ecuador Tax/SRI Domain Agent
- Property and Ownership Agent
- Reservations and Security Operations Agent
- Governance Agent

A local agent must not redefine common architecture, security, QA or DevOps rules.

## Branch strategy

- `main`: stable integration/release line.
- `foundation/*`: initial platform work.
- `feature/sXX-STORY-description`: functional stories.
- `fix/sXX-STORY-description`: defects.
- `chore/*`: non-functional controlled maintenance.
- `release/x.y.z`: only when release hardening requires it.
- `hotfix/x.y.z`: production critical fixes.

Prefer squash merge so each story produces one traceable main-line commit.

## PR gates

A PR cannot merge until applicable gates pass:

- backend restore/build
- backend unit tests
- architecture tests
- integration tests
- Angular lint/test/build
- Docker image build
- secret scan
- dependency/vulnerability scan
- tenant-isolation tests for tenant-owned features
- migration validation for database changes
- documentation/task-index update

## Environment strategy

### Local

Docker Compose with developer-facing ports and local secrets outside Git.

### DEV

Automatic deployment after merge to main once CI is stable.

### TEST/UAT

Promotion of immutable images from DEV; no rebuild.

### PROD

Explicit approval, database backup/restore plan, migration precheck and rollback/runbook.

Environment configuration is externalized. Images remain identical across environments.

## Docker topology

Initial services:

- appcondominio-web
- appcondominio-api
- appcondominio-worker
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

Initial default: shared application/database infrastructure with tenant-owned rows explicitly scoped by TenantId and CommunityId where applicable.

Architecture must leave room for selected tenants to move to dedicated databases later without changing business use cases.

Required protections:

- authenticated tenant context
- server-side tenant authorization
- automatic query/write scoping where safe
- explicit cross-tenant administration paths
- automated isolation tests
- tenant-aware cache keys
- tenant-aware events and jobs

## Integration strategy

PortalCorporativo capabilities are integrated through local application ports/adapters. Domain code never references remote HTTP details.

Examples:

- `IIdentityContext`
- `IAuditService`
- `INotificationService`
- `IContentService`
- `IConfigurationService`
- `ICatalogService`
- `IReportingGateway`
- `IIntegrationGateway`

If PortalCorporativo exposes an official shared contract/package, prefer that contract instead of duplicating DTOs.

## Sprint roadmap

### Sprint 1 - Foundation
Repository, architecture, .NET/Angular skeleton, SQL Server, Docker, Redis, RabbitMQ, Worker, observability, CI and architecture tests.

### Sprint 2 - Portal integration
Security, tenant/user context, permissions, menu, audit, notifications, content, catalogs and configuration.

### Sprint 3 - SaaS
Organizations, plans, subscriptions, white-label and module limits.

### Sprint 4 - Communities
Community master data, hierarchy, tax setup, accounts, authorities and documents.

### Sprint 5 - Properties
Units, parking/storage, measurements, coefficients and versioning.

### Sprint 6 - People/Ownership
Owners, ownership history, tenants, residents, activation and expiration.

### Sprint 7 - Billing engine
Charge concepts, formulas, interests, discounts, simulation and approval.

### Sprint 8 - Monthly generation
Periods, draft calculation, validation, approval, issuance, statements and notifications.

### Sprint 9 - Collections
Payments, allocation, balances, reversals, aging, agreements and automated collection.

### Sprint 10 - Banking and SRI
Bank imports/reconciliation and Ecuador electronic-document orchestration.

### Sprint 11 - Accounting
Chart of accounts, journals, ledgers, automatic entries, trial balance and close.

### Sprint 12 - Treasury
Suppliers, AP, approvals, payments, petty cash, advances and withholding.

### Sprint 13 - Budget/reporting
Annual budget, versions, reforms, availability and financial statements.

### Sprint 14 - Procurement
Requests, quotations, comparison, approvals, purchase orders, receipt and contracts.

### Sprint 15 - Maintenance
Assets, preventive plans, work orders, technicians, evidence, cost and PWA operations.

### Sprint 16 - Reservations/security
Common-area reservations, guarantees, visitors, QR/PIN, vehicles, packages and incidents.

### Sprint 17 - Governance
Rules, infractions, appeals, assemblies, quorum, voting, minutes and claims.

### Sprint 18 - Production readiness
E2E, security, load, SQL tuning, backup/restore, monitoring, runbooks, training and v1.0.

## Release milestones

- R0: Sprint 2 - technical platform integrated with PortalCorporativo.
- R1: Sprint 6 - SaaS and condominium/property/person core.
- R2: Sprint 10 - billing, collections, banking and SRI.
- R3: Sprint 13 - financial management.
- R4: Sprint 17 - complete operational/governance scope.
- R5: Sprint 18 - production v1.0.

## Sprint execution pattern

Every story follows:

1. Classify reuse.
2. Confirm architecture impact.
3. Create branch.
4. Implement smallest vertical slice.
5. Add unit/integration/architecture tests.
6. Validate Docker if runtime changed.
7. Validate tenant isolation/security.
8. Update API/docs/task index.
9. Open PR.
10. Review, validate and squash merge.

## First implementation sequence

The first code-producing tasks should be:

1. S01-01 repository standards and solution skeleton.
2. S01-02 backend host + modular bootstrapper.
3. S01-03 reference Organizations module.
4. S01-04 SQL Server + migrations.
5. S01-05 Angular shell integration skeleton.
6. S01-06 Docker Compose.
7. S01-07 RabbitMQ/Redis/Worker.
8. S01-08 observability/error handling.
9. S01-09 automated test foundation.
10. S01-10 GitHub Actions and security gates.
