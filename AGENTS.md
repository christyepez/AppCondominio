# AGENTS.md - AppCondominio

## Purpose

AppCondominio is a SaaS multi-tenant condominium-management domain application. This repository MUST use `christyepez/CodexCommonAgents` as the common agent/rules repository and `christyepez/PortalCorporativo` as the transverse platform.

## Mandatory reading order

Before changing code, Codex MUST read only the minimum context required, in this order:

1. `AGENTS.md` in this repository.
2. `codex/PROJECT_CONTEXT.md`.
3. `codex/TASK_INDEX.md` for the active story.
4. `CodexCommonAgents/AGENTS.md`.
5. `CodexCommonAgents/registry/reusable-portal-apis.md`.
6. `CodexCommonAgents/registry/do-not-duplicate.md`.
7. `CodexCommonAgents/playbooks/portal-first-implementation.md`.
8. `CodexCommonAgents/agents/03-backend-agent.md` for backend work.
9. The common agent assigned by the Coordinator Agent.
10. A domain-specific file under `codex/agents/` only when required.

## Mandatory backend runtime

All backend components in AppCondominio MUST target:

```text
.NET 10
ASP.NET Core 10
TargetFramework: net10.0
```

This applies to API projects, application/domain/infrastructure libraries, workers, tests, EF Core migrations, Docker images and CI jobs. Stable 10.x packages must be used; preview framework packages are not allowed unless explicitly approved.

## Mandatory classification

Every task MUST declare one classification before implementation:

- `REUSE`: use an existing PortalCorporativo capability unchanged.
- `EXTEND`: extend portal metadata, menus, permissions, catalogs, configuration or supported extension points.
- `ADAPT`: implement an AppCondominio adapter to a reusable portal API.
- `CREATE`: implement condominium-domain capability not provided by PortalCorporativo.
- `BLOCKED`: do not implement until the dependency or contract is resolved.

## Portal-first rule

Do not create AppCondominio implementations for authentication, global users, global roles, permission engine, menu engine, global configuration, global catalogs, audit engine, notification engine, file/content engine, reporting platform, integration platform, API gateway or transverse observability when PortalCorporativo already provides the capability.

## Architecture

Required architectural style:

- .NET 10 / ASP.NET Core 10
- Angular aligned with PortalCorporativo
- SQL Server
- Redis
- RabbitMQ
- Docker Compose
- Modular Monolith
- Clean Architecture per bounded context
- Vertical Slice for use cases
- DDD where domain complexity justifies it
- pragmatic CQRS
- domain events
- integration events
- transactional outbox/inbox
- idempotent consumers

## Multi-tenancy

Every tenant-owned business entity and use case MUST enforce tenant isolation in the backend. `TenantId` and, when applicable, `CommunityId` are security boundaries, not UI filters.

No query or command may trust tenant/community identifiers supplied only by the client without validating them against the authenticated context.

## Bounded contexts

Initial bounded contexts:

- Organizations
- Communities
- Properties
- Ownership
- People
- Billing
- Collections
- Accounting
- Treasury
- Budgeting
- Procurement
- Maintenance
- Reservations
- SecurityOperations
- Governance
- Tax
- Reporting
- Integrations

## Source-code dependency rules

- Domain has no dependency on Infrastructure or HTTP.
- Application depends on its Domain and explicit contracts.
- Infrastructure implements application/domain ports.
- API endpoints are thin and contain no business logic.
- A module never directly reads or writes another module's tables.
- Cross-module communication uses explicit application contracts or events.
- SharedKernel stays small and contains no condominium business workflow.

## Branch and PR rules

- `main` is the integration and release line.
- Work is performed on story/foundation branches.
- One story or tightly-related technical slice per branch/PR.
- Prefer squash merge after validation.
- Never bypass tests to merge.

Branch convention:

`<type>/<sprint>-<story>-<short-description>`

## Definition of Done

A story is complete only when applicable checks pass:

- Portal capability classification recorded.
- Architecture boundaries respected.
- Authorization enforced server-side.
- Tenant isolation validated.
- Input validation implemented.
- Audit impact assessed.
- Notification impact assessed.
- Configuration/menu impact assessed.
- Unit tests pass.
- Integration tests pass.
- Architecture tests pass.
- Docker build succeeds.
- No secrets committed.
- Documentation and `codex/TASK_INDEX.md` updated.

## Required Codex task output

Every implementation task must finish with:

```text
Agent:
Task:
Story:
Sprint:
Runtime: .NET 10
Base Commit:
Branch:
Commit:
Files Read:
Files Created:
Files Modified:
Portal Capability Checked:
Reuse Classification:
Portal Components Reused:
Portal Components Extended:
Adapters Created:
New Domain Components:
Security Impact:
Tenant Isolation:
Audit Impact:
Notification Impact:
Menu Impact:
Configuration Impact:
Tests Added:
Commands Executed:
Validation:
Risks:
Next Step:
```
