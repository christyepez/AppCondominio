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
| UI-PWA | Installable PWA shell / static offline safety | VALIDATED in stacked UI chain through PR #41 / CI `34379512223` |
| UI-14 | Cross-channel E2E, selectors/workflows and UX hardening | VALIDATED - PR #41 / CI `34379512223` |
| UI-15 | Isolated-runtime dashboard / Portal-unavailable UX | IN VALIDATION |
| UI-16 | Guard Portal guided operational workflow | IN VALIDATION |
| UI-17 | Supplier Portal bid workflow hardening | IN VALIDATION |

Current validated delivery head: `feature/final-integration-security-adr` / PR #49 / SHA `7aa4b43bbd0e01f3d72c74fd0c46de01bbcfe996`, validated by CI `34502095940` (run #601). The Angular 21 Admin, Resident, Supplier and Guard channels are implemented in the stacked UI chain. Authorized selector contracts preserve backend tenant/community validation and explicit 401/403 behavior without creating a local authentication/token engine.

First-pass Admin screens intentionally allow explicit GUID entry where the backend does not yet expose safe list/search contracts. Subsequent UX hardening will replace those fields with selectors only where real authorized lookup endpoints are available; it must not invent cross-tenant lookup behavior.

## Product-completeness note
The 18-sprint backend/platform roadmap is complete and validated except for explicitly blocked Portal-owned capabilities. The application-owned backend/platform, all four end-user UI channels and release-candidate operational evidence are validated through PR #49 / CI `34502095940`. Product release remains conditional on the explicitly blocked Portal-owned capabilities required by the intended deployment scope and on explicit environment/infrastructure approval.

## Release rule
A release is not production-ready merely because code CI is green. Production requires explicit environment/infrastructure approval, restorable backup evidence, migration precheck, Portal dependency readiness for the intended scope, production secrets supplied outside Git, SRI certification/provider configuration where Tax issuance is enabled, smoke tests and applicable manual critical journeys from `docs/TEST_PLAN.md`.

- UI-14B - Authorized unit/area selectors: VALIDATED via stacked PR #41 / CI `34379512223` - administrative Properties unit lookup and Reservations area lookup exposed with existing read permissions; Billing, Collections, People and Reservations consume selector options.

- UI-14C - Authorized organization selectors: VALIDATED via stacked PR #41 / CI `34379512223` - organization list query/endpoint plus selectors in Organizations, Communities and SaaS; no Portal ownership duplicated.

- UI-14D - Authorized procurement supplier/round selectors: VALIDATED via stacked PR #41 / CI `34379512223` - community-scoped active suppliers and open rounds exposed with Procurement.Read and consumed by admin procurement UI.

- UI-14E Budgeting/Tax selectors: VALIDATED via stacked PR #41 / CI `34379512223`. Budget plans and tax documents now use community-scoped authorized lookups; no raw PlanId/DocumentId entry remains in these admin flows.

- UI-14F People selectors: VALIDATED via stacked PR #41 / CI `34379512223`. Security host and Reservations requester now consume an explicit People.Read API contract; no direct cross-context table access.

- UI-14G Cross-channel defense-in-depth: VALIDATED via stacked PR #41 / CI `34379512223`. Added IPeopleDirectory integration contract and automated negative tests proving Reservations/Security reject cross-community person IDs even when UI selectors are bypassed. Portal User directory remains an explicit Portal-owned follow-up.

- UI-14H Treasury/Procurement supplier integration: VALIDATED via PR #41 / CI `34379512223`. Treasury resolves eligible suppliers through `IProcurementSupplierDirectory`, exposes a Treasury.Read-scoped selector endpoint and rejects cross-community supplier payloads without direct Procurement table access.


## Follow-on phase - Release Candidate hardening

| Order | Capability | Status |
|---:|---|---|
| RC-01 | Docker runtime smoke / fail-closed readiness verification | VALIDATED - PR #42 / CI `34388329936` |
| RC-02 | Database migration inventory / backup-gated apply evidence | VALIDATED - PR #43 / CI `34403370897` |
| RC-03 | SQL backup/restore rehearsal evidence in release stack | VALIDATED - PR #44 / CI `34406992413` |
| RC-04 | Immutable image/release evidence manifest | VALIDATED - PR #45 / CI `34408707368` |
| RC-05 | Previous immutable image-set runtime rollback rehearsal | VALIDATED - PR #46 / CI `34409823073` |
| RC-06 | Migration failure forward-fix / backup-restore recovery rehearsal | VALIDATED - PR #47 / CI `34411290846` |
| RC-FINAL | Final integration, ADR continuity and cumulative release validation | VALIDATED - PR #49 / CI `34502095940` |
| RC-07 | Docker Hub immutable runtime portability / pull-only local deployment | IN VALIDATION |
