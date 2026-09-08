# AppCondominio - Codex Task Index

## Delivery status

| Sprint | Scope | Status | Evidence |
|---:|---|---|---|
| 01 | Foundation / architecture | VALIDATED | CI `34161344428`, PR #1 |
| 02 | PortalCorporativo integration | PARTIAL / BLOCKED EXTERNAL | S02-01/03/04/10 validated; S02-02/05/06/07/08/09 blocked by Portal contracts |
| 03 | SaaS core | VALIDATED | CI `34176135388`, PR #12 |
| 04 | Communities | VALIDATED | CI `34231318775`, PR #13 |
| 05 | Properties / allocation coefficients | VALIDATED | CI `34232684016`, PR #14 |
| 06 | People / ownership / residents | VALIDATED | CI `34236175554`, PR #15 |
| 07 | Billing charge engine | VALIDATED | CI `34237553385`, PR #16 |
| 08 | Monthly billing generation | VALIDATED | CI `34242504016`, PR #17 |
| 09 | Collections / receivables / payments | VALIDATED | CI `34243645443`, PR #18 |
| 10 | Banking / reconciliation / Tax-SRI adapter | VALIDATED | CI `34247469067`, PR #19 |
| 11 | Accounting | VALIDATED | CI `34248668439`, PR #20 |
| 12 | Treasury / Accounts Payable | VALIDATED | CI `34250192582`, PR #21 |
| 13 | Budgeting / reporting | VALIDATED | CI `34251525621`, PR #22 |
| 14 | Procurement | VALIDATED | CI `34252671286`, PR #23 |
| 15 | Maintenance / assets | VALIDATED | CI `34255713507`, PR #24 |
| 16 | Reservations / security operations | VALIDATED | CI `34257015439`, PR #25 |
| 17 | Governance / coexistence | VALIDATED | CI `34261485542`, PR #26 |
| 18 | Production readiness | VALIDATED | CI `34263049464`, PR #27 |

## Sprint 01 - Architecture and foundation
All S01-01 through S01-10 are VALIDATED: .NET 10 solution, modular bootstrapper, Organizations reference module, SQL Server/EF Core 10, Angular 21 LTS shell, Docker Compose, Redis/RabbitMQ/Worker, observability, automated tests and CI/security gates.

## Sprint 02 - PortalCorporativo integration

| Story | Capability | Status | Reason / evidence |
|---|---|---|---|
| S02-01 | Portal-compatible JWT validation and identity | VALIDATED | PR #2, CI `34161895440` |
| S02-02 | User/Tenant/Community context | BLOCKED | ADR-002; Portal lacks safe non-admin trusted tenant/community context |
| S02-03 | Permission authorization | VALIDATED | PR #4, CI `34162705886` |
| S02-04 | Menu registration | VALIDATED | PR #5, CI `34163039648` |
| S02-05 | Audit integration | BLOCKED | ADR-003; trusted service identity/actor enforcement required |
| S02-06 | Notification integration | BLOCKED | ADR-004; tenant-aware service identity required |
| S02-07 | Content/File integration | BLOCKED | ADR-005; Portal Content contract/API incomplete |
| S02-08 | Catalog integration | BLOCKED | ADR-006; Portal Catalog contract/API incomplete |
| S02-09 | Configuration integration | BLOCKED | ADR-007; Portal currently hard-codes tenant `default` |
| S02-10 | Distributed health/resilience | VALIDATED | PR #11, CI `34164462180` |

Blocked Portal capabilities must not be replaced with insecure local duplicates. See `docs/DEPENDENCY_MATRIX.md`.

## Sprint 03 - SaaS
Validated commercial tenant/organization profile, white-label configuration, plans, subscriptions, module entitlements, usage thresholds, suspension/reactivation and Shared/Dedicated database strategy using secret references rather than persisted raw credentials.

## Sprint 04 - Communities
Validated community master data, legal/tax/address configuration, independent `communities` schema and persistence boundaries.

## Sprint 05 - Properties
Validated property types, units, linked parking/storage, areas, allocation coefficient calculation/versioning/approval and demo generation of 300 units with total coefficients exactly 100%.

## Sprint 06 - People / Ownership / Residents
Validated natural/legal persons, ownership percentages/history, leases/residents, financial responsibility, hashed activation codes, activation requests/grants and access expiration semantics.

## Sprint 07 - Billing engine
Validated charge concepts, versioned formulas, fixed/area/coefficient/consumption/percentage/proration/manual calculation modes, discounts, interest, boundaries, accounting references and simulation without receivable creation.

## Sprint 08 - Monthly billing
Validated billing periods, draft generation, inconsistency detection, approval, issuance, obligations, reversal rules, statements, integration event and Worker-controlled scheduled issuance of approved periods.

## Sprint 09 - Collections
Validated receivables, idempotent payments, partial/full application, unapplied balances, payment reversal and aging.

## Sprint 10 - Banking / Reconciliation / Tax
Validated bank accounts, idempotent movement import, reconciliation suggestions/confirmation and Tax electronic-document lifecycle. SRI transport remains an external production configuration; without it the adapter returns `PendingConfiguration` and never fakes authorization.

## Sprint 11 - Accounting
Validated chart of accounts, periods, balanced/idempotent postings, ledger, trial balance, close and reversal through compensating entries while preserving original history.

## Sprint 12 - Treasury / AP
Validated supplier payables, payment requests, approvals/rejections, partial disbursements, reversal, outstanding balances and cash forecast.

## Sprint 13 - Budgeting / Reporting
Validated annual/monthly budgets, approval immutability, idempotent actuals, budget-vs-actual variance and executive/YTD summary.

## Sprint 14 - Procurement
Validated community suppliers, requisitions, quotation rounds, bids, scoring/ranking, award and purchase order references without direct Treasury table writes.

## Sprint 15 - Maintenance
Validated asset registry, preventive plans, incidents, work orders, assignment/start/complete lifecycle, plan rescheduling, costs, Procurement references and maintenance KPI.

## Sprint 16 - Reservations / Security Operations
Validated reservable areas, capacity/fees, booking lifecycle and overlap prevention; visitor authorizations, gate check-in/out, security incidents, escalation/resolution and KPI in independent schemas.

## Sprint 17 - Governance
Validated assemblies/quorum, motions/voting, coexistence cases, penalties by Billing reference only, governance KPI and independent `governance` schema.

## Sprint 18 - Production readiness
Validated in final CI `34263049464`:
- hardened Nginx proxy/security headers;
- release prerequisite validation script;
- deployment smoke-test script;
- production deployment/migration/rollback runbook;
- external dependency readiness matrix;
- end-to-end product test plan;
- CI production-readiness gate;
- final full-stack build, tests, security scans and Docker image build.

## Follow-on phase - UI Completion

| Order | Capability | Status |
|---:|---|---|
| UI-01 | Admin shell, responsive navigation and live dashboard | IN VALIDATION |
| UI-02 | Communities operational list/create | IN VALIDATION |
| UI-03 | Properties / aliquot validation and 300-unit pilot tool | IN VALIDATION |
| UI-04 | Governance KPI | IN VALIDATION |
| UI-05 | People / ownership / residents UI | NEXT |
| UI-06 | Billing / monthly generation / collections UI | PLANNED |
| UI-07 | Banking / Tax / Accounting / Treasury UI | PLANNED |
| UI-08 | Budgeting / Procurement / Maintenance UI | PLANNED |
| UI-09 | Reservations / Security Operations UI | PLANNED |
| UI-10 | Resident portal | PLANNED |
| UI-11 | Supplier portal | PLANNED |
| UI-12 | Guard portal | PLANNED |
| UI-13 | Cross-channel E2E and UX hardening | PLANNED |

Current branch: `feature/ui-completion-admin-core`. The first UI increment exposes a real Communities list use case under `appcondominio.communities.read` and consumes only existing secured backend contracts. Authentication/token issuance remains Portal-owned; the UI shows explicit 401/403 states rather than inventing a local login engine.

## Product-completeness note
The 18-sprint backend/platform roadmap is complete and validated except for explicitly blocked Portal-owned capabilities. UI completion is a follow-on delivery phase and must be completed before calling all end-user channels (Admin, Resident, Supplier and Guard) product-complete.

## Release rule
A release is not production-ready merely because code CI is green. Production requires explicit environment/infrastructure approval, restorable backup evidence, migration precheck, Portal dependency readiness for the intended scope, production secrets supplied outside Git, SRI certification/provider configuration where Tax issuance is enabled, smoke tests and applicable manual critical journeys from `docs/TEST_PLAN.md`.
