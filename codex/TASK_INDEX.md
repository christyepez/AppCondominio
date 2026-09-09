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

## Sprint 02 external blockers

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

## Backend/platform completion
Sprints 01-18 validate the Modular Monolith foundation, SaaS, communities, properties, people/ownership, billing, collections, banking/Tax, accounting, treasury, budgeting/reporting, procurement, maintenance, reservations/security, governance and production-readiness controls. Financial/legal corrections preserve history; bounded contexts do not directly mutate one another's tables. SRI remains fail-closed as `PendingConfiguration` until an approved provider/certificate is configured.

## Follow-on phase - UI Completion

| Order | Capability | Status |
|---:|---|---|
| UI-01 | Admin shell, responsive navigation, live dashboard/session state | VALIDATED - PR #28 |
| UI-02 | Organizations + SaaS plans/entitlements administration | VALIDATED - PR #28 |
| UI-03 | Communities list/create | VALIDATED - PR #28 |
| UI-04 | Properties / aliquot validation / 300-unit pilot tool | VALIDATED - PR #28 |
| UI-05 | People / owners / residents operational UI | VALIDATED - PR #28 |
| UI-06 | Billing / monthly periods / statements | VALIDATED - PR #28 |
| UI-07 | Collections / aging / payment registration | VALIDATED - PR #28 |
| UI-08 | Banking / Tax-SRI / Accounting / Treasury | VALIDATED - PR #28 |
| UI-09 | Budgeting / Procurement / Maintenance | VALIDATED - PR #28 |
| UI-10 | Reservations / Security Operations / Governance | VALIDATED - PR #28 |
| UI-11 | Resident portal | VALIDATED - PR #29 |
| UI-12 | Supplier portal | VALIDATED - PR #30 |
| UI-13 | Guard portal | VALIDATED - PR #31 |
| UI-PWA | Installable PWA shell / static offline safety | IN VALIDATION - PR #32 |
| UI-14 | Cross-channel E2E, selectors/workflows and UX hardening | IN PROGRESS |

Current branch: `feature/ui-completion-admin-core`. Admin Core now exposes a navigable Angular 21 console across all principal bounded contexts and consumes real secured backend endpoints. The only new backend read surface added for UI completion is `GET /api/communities/`, protected by the existing `appcondominio.communities.read` permission. Screens preserve explicit 401/403 behavior and do not create a local authentication/token engine.

First-pass Admin screens intentionally allow explicit GUID entry where the backend does not yet expose safe list/search contracts. Subsequent UX hardening will replace those fields with selectors only where real authorized lookup endpoints are available; it must not invent cross-tenant lookup behavior.

## Product-completeness note
The 18-sprint backend/platform roadmap is complete and validated except for explicitly blocked Portal-owned capabilities. Admin Core is currently in final validation. Resident, Supplier and Guard channels still require their dedicated UI delivery before all end-user channels can be called product-complete.

## Release rule
A release is not production-ready merely because code CI is green. Production requires explicit environment/infrastructure approval, restorable backup evidence, migration precheck, Portal dependency readiness for the intended scope, production secrets supplied outside Git, SRI certification/provider configuration where Tax issuance is enabled, smoke tests and applicable manual critical journeys from `docs/TEST_PLAN.md`.

- UI-14B - Authorized unit/area selectors: IN VALIDATION - administrative Properties unit lookup and Reservations area lookup exposed with existing read permissions; Billing, Collections, People and Reservations consume selector options.

- UI-14C - Authorized organization selectors: IN VALIDATION - organization list query/endpoint plus selectors in Organizations, Communities and SaaS; no Portal ownership duplicated.
