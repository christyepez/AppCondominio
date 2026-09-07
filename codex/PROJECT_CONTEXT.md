# AppCondominio - Project Context

## Product

Commercial name: **Conjunto al Dia**.

AppCondominio is a SaaS, multi-company, white-label condominium-management platform. The first target country is Ecuador, with internationalization prepared from the architecture.

## Initial pilot assumptions

- No legacy migration is required for the first pilot.
- Initial demonstration scope: 2 condominium communities.
- Initial demonstration volume: 300 units per community.
- Each community may use its own RUC, administrator and tax configuration.

## User channels

- Administrator portal.
- Resident portal.
- Supplier portal.
- Guard/operations portal.
- Responsive/PWA support.

## Runtime target

All components must run with Docker Compose and be portable between cloud and on-premises installations.

## Technology baseline

Use the same technology family as PortalCorporativo, with the AppCondominio backend standardized on .NET 10:

- .NET 10 / ASP.NET Core 10
- Angular
- SQL Server
- Redis
- RabbitMQ
- Docker / Docker Compose
- Serilog
- Seq
- OpenTelemetry
- GitHub Actions

All backend projects, workers, tests, migrations, containers and CI build jobs target `net10.0`. Stable .NET 10 servicing packages are required; preview framework packages are not part of the production baseline.

## Architecture baseline

- Modular Monolith initially.
- Clean Architecture inside bounded contexts.
- Vertical Slice per use case.
- DDD for aggregates/value objects/domain invariants where useful.
- Pragmatic CQRS.
- Domain events and integration events.
- Transactional Outbox/Inbox.
- Idempotent asynchronous consumers.
- API contracts isolated from internal domain models.
- Designed so bounded contexts can later be extracted as services if business or scaling pressure requires it.

## Transverse platform

Repository: `christyepez/PortalCorporativo`.

Portal-first capabilities include authentication/security, users/roles/permissions, menus, configuration, catalogs, audit, notifications, file/content management, reporting, integrations, gateway, transverse workers and outbox support where available.

AppCondominio must integrate through contracts/adapters and must not duplicate these engines.

## Common Codex agents

Repository: `christyepez/CodexCommonAgents`.

Common agents/rules/playbooks are authoritative for cross-project architecture, security, quality, DevOps and PortalCorporativo reuse. Backend work must additionally apply `agents/03-backend-agent.md`. AppCondominio stores only local context and condominium-domain extensions.

## Domain bounded contexts

1. Organizations - SaaS tenant/administrator companies/subscriptions.
2. Communities - condominiums, buildings, blocks, configuration and authorities.
3. Properties - units, parking, storage, areas and allocation coefficients.
4. Ownership - ownership history and owner relationships.
5. People - residents, tenants, dependents and contacts.
6. Billing - charge concepts, formulas, monthly fees and extraordinary charges.
7. Collections - payments, balances, aging, agreements and collection workflows.
8. Accounting - chart of accounts, journals, ledgers and closing.
9. Treasury - suppliers, accounts payable, payments and cash management.
10. Budgeting - budgets, versions, reforms and execution control.
11. Procurement - requests, quotations, purchase orders and contracts.
12. Maintenance - assets and preventive/corrective work orders.
13. Reservations - common-area availability, reservations and guarantees.
14. SecurityOperations - visitors, access, vehicles, packages and incidents.
15. Governance - rules, infractions, assemblies, voting, minutes and claims.
16. Tax - Ecuador electronic-document orchestration and tax-domain rules; reuse Portal integrations when available.
17. Reporting - domain report definitions/adapters to PortalCorporativo reporting.
18. Integrations - condominium-domain adapters and external contracts; reuse Integration API where applicable.

## Roadmap

18 two-week sprints:

1. Architecture, repository and Docker foundation.
2. PortalCorporativo integration.
3. SaaS organizations, plans and subscriptions.
4. Communities and physical structure.
5. Properties, measurements and allocation coefficients.
6. Owners, residents and occupancy.
7. Charge calculation engine.
8. Monthly fee generation.
9. Payments, receivables and collections.
10. Bank reconciliation and SRI.
11. General accounting.
12. Treasury and accounts payable.
13. Budget and financial reporting.
14. Procurement and contracts.
15. Assets and maintenance.
16. Reservations and security operations.
17. Governance and community coexistence.
18. Stabilization, security and production readiness.

## Non-negotiable rules

- Backend runtime is .NET 10 / ASP.NET Core 10.
- Tenant isolation is enforced in backend code and tests.
- No cross-module table access.
- No secrets in Git.
- No direct duplication of PortalCorporativo transverse capabilities.
- All critical write operations must be auditable.
- External integrations need timeout, retry where safe, idempotency and failure handling.
- Financial operations are never physically deleted after posting; use reversals/corrections.
- Dates stored in UTC unless a legal/accounting rule explicitly requires a local business date.
