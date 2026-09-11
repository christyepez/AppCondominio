# Conjunto al DÃƒÂ­a - Product Test Plan

## 1. Test prerequisites
- Start SQL Server, Redis, RabbitMQ, Seq, API, Worker and Web.
- Configure a valid Portal-compatible JWT for protected API tests.
- Provision AppCondominio Portal permissions/menu in the intended test environment.
- Use two independent organizations/communities to verify isolation behavior explicitly.
- Keep SRI in `PendingConfiguration` unless using an approved certification environment and test certificate.

## 2. Platform and infrastructure
- `GET /` returns service information and HTTP 200.
- `GET /health/live` returns 200 while API process is alive.
- `GET /health/ready` returns 200 only when SQL contexts, Redis, RabbitMQ and Portal gateway are reachable.
- Stop Redis: liveness remains healthy; readiness fails.
- Restore Redis: readiness returns healthy without application redeploy.
- Stop RabbitMQ and repeat the readiness behavior.
- Stop SQL Server and verify readiness fails without exposing connection-string details.
- Verify API responses include/carry correlation ID behavior through logs.
- Verify Web loads through Nginx and security headers include `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` and `Permissions-Policy`.

## 3. Authentication and authorization
- Protected endpoint without JWT returns 401.
- Valid signed JWT without the required `permission` claim returns 403.
- JWT with `*.read` may query read endpoints but may not call manage endpoints.
- JWT with `*.manage` may execute the corresponding write operations.
- Expired JWT is rejected.
- JWT with wrong issuer/audience/signature is rejected.
- Confirm no endpoint trusts client-supplied tenant/community headers as authenticated context.

## 4. SaaS administration
- Create organization/administrator profile.
- Configure white-label branding fields.
- Create a plan and activate a subscription.
- Enable/disable modules without redeploy.
- Validate usage thresholds at 80% and 90%.
- Suspend tenant and verify state changes are preserved historically.
- Reactivate tenant.
- Configure Shared database strategy.
- Configure Dedicated strategy using only a secret reference; raw connection string must not be persisted as domain data.

## 5. Communities
- Create two communities under different organizations.
- Configure legal/tax/address information.
- Verify codes/identifiers that must be unique cannot be duplicated in the same scope.
- Store document metadata/reference without requiring a local file-storage implementation.
- Verify community A records are not accidentally returned when querying community B identifiers in module-specific APIs.

## 6. Properties and allocation coefficients
- Define property types.
- Generate sample data for a community with 300 units.
- Verify exactly 300 primary units are generated.
- Verify parking/storage relationships are valid.
- Verify areas are positive and required fields cannot be blank.
- Calculate coefficients and verify approved coefficients total exactly 100%.
- Create a new coefficient version and verify previous version remains historical.
- Reject invalid coefficient totals before approval.

## 7. People, ownership and residents
- Create natural person and legal entity profiles.
- Assign multiple owners to one property and verify ownership percentages.
- Reject invalid ownership percentage totals/ranges.
- Close an ownership period and create a new owner, preserving history.
- Register a tenant/resident with validity dates.
- Mark financial responsible party.
- Generate activation code and verify stored representation is a hash, not plain text.
- Submit activation request, approve it and verify access grant lifecycle.
- Expire resident access and verify the local grant is no longer active.

## 8. Billing charge engine
- Create charge concepts and version formulas.
- Validate fixed amount calculation.
- Validate calculation by area.
- Validate calculation by allocation coefficient.
- Validate consumption and percentage methods where configured.
- Validate minimum/maximum boundaries.
- Validate discounts and interest-period calculation.
- Approve a formula/version and ensure later edits require a new version.
- Run simulation and verify no receivable is created by simulation.
- Verify accounting mapping reference is retained without writing Accounting tables directly.

## 9. Monthly billing generation
- Open a billing period.
- Generate draft charges.
- Verify draft generation does not create receivables/obligations prematurely.
- Introduce an inconsistency and verify issue detection blocks approval/emission.
- Approve a clean draft.
- Issue the period and verify obligations are created exactly once.
- Re-run issuance and verify idempotent behavior/no duplicate obligations.
- Generate account statement.
- Reverse an eligible issuance and verify status/history.
- Verify reversal is blocked when payment state makes reversal unsafe.
- Verify Worker only auto-issues approved periods whose due execution time has arrived.

## 10. Collections
- Mirror/import a Billing obligation using its external/reference identifier.
- Register payment with unique transaction/reference key.
- Retry the same payment and verify idempotency.
- Apply partial payment and verify remaining receivable balance.
- Apply full payment and verify receivable closes.
- Record unapplied amount and verify it remains available for later application.
- Reverse a payment and verify receivable balance is restored.
- Run aging report and verify bucket assignment using due dates.

## 11. Banking and reconciliation
- Create community bank account.
- Import bank movements using an import key.
- Re-import the same movement and verify no duplicate row.
- Generate reconciliation suggestions using amount/date/reference.
- Verify confidence/selection behavior.
- Confirm reconciliation and ensure only explicit references cross to Collections.
- Reject reconciliation of already-reconciled movement where applicable.

## 12. Tax / SRI
- Create electronic-document lifecycle record.
- Without provider configuration, submission must result in `PendingConfiguration`, never a fake authorization.
- Confirm certificate/password/credentials are not persisted in domain entities or committed configuration.
- In an approved SRI certification environment, submit a valid test document and record provider response/authorization lifecycle.
- Test provider rejection and retry/status update behavior without duplicating document identity.

## 13. Accounting
- Configure chart of accounts.
- Open accounting period.
- Post balanced journal entry and verify debit equals credit.
- Reject unbalanced entry.
- Reject duplicate source reference.
- Query ledger and trial balance.
- Close accounting period and reject new posting into that period.
- Reverse a posted entry and verify original remains historical while compensating entry nets the balance.

## 14. Treasury / Accounts Payable
- Create supplier payable/invoice.
- Create payment request.
- Approve request and reject amount above outstanding balance.
- Disburse partial payment with unique payment reference.
- Verify outstanding payable balance.
- Run cash forecast grouped by due date.
- Reverse disbursement and verify payable balance restoration.
- Verify no direct cross-write from Procurement into Treasury tables.
- Verify Treasury lists only active suppliers for the selected community and rejects payable creation when SupplierId belongs to another community.

## 15. Budgeting and reporting
- Create annual budget and monthly allocations.
- Approve budget and verify approved budget is immutable.
- Load actuals with idempotent source reference.
- Retry the same actual and verify no duplication.
- Compare budget vs actual and validate variance arithmetic.
- Validate YTD/executive summary.

## 16. Procurement
- Create two suppliers in one community.
- Prevent supplier from being used in another community where eligibility does not match.
- Create and approve requisition.
- Open sourcing round.
- Submit two bids; prevent duplicate bid from same supplier/round.
- Score bids 0-100 and verify ranking.
- Close round before award.
- Award evaluated bid and create purchase order reference.
- Verify Procurement does not create Treasury payables directly.

## 17. Maintenance and assets
- Register asset.
- Create preventive plan and due date.
- Report corrective incident.
- Create work order, assign, start and complete it.
- Verify incident resolves when linked work order is completed where configured.
- Verify next preventive date advances after execution.
- Record actual maintenance cost.
- Validate maintenance KPI: assets, open incidents, due plans, open work orders and completed cost.

## 18. Reservations
- Configure reservable common area with capacity and fee.
- Request booking inside capacity.
- Reject booking above capacity.
- Reject overlapping pending/confirmed booking.
- Confirm booking, cancel another booking and complete an executed booking.
- Verify one community cannot use an area's identifier from another community as if it belonged locally.

## 19. Security operations
- Create visitor authorization with validity window.
- Prevent duplicate/overlapping active authorization for same visitor/community where rule applies.
- Check visitor in at a gate.
- Check visitor out.
- Verify checked-in KPI changes correctly.
- Create security incident, escalate and resolve it.
- Verify resolved incident is not counted as open/escalated.

## 20. Governance and coexistence
- Create assembly with quorum percentage.
- Attempt to open below quorum and verify rejection.
- Open at/above quorum.
- Create motion, cast yes/no/abstain votes and close it.
- Verify approval result is derived from closed vote totals.
- Reject votes after motion closes.
- Report coexistence case, move to review and resolve it.
- Add penalty using only a Billing reference; verify Governance does not write Billing tables directly.
- Validate governance KPI.

## 21. Cross-tenant / negative tests
- Repeat critical create/read/update paths with community A and B IDs intentionally mixed; verify business eligibility checks reject invalid cross-community relationships.
- Attempt duplicate external/import/payment/source references and verify idempotent rejection/no duplicate.
- Send malformed GUIDs, negative amounts, invalid percentages, invalid date ranges and oversized text; expect 4xx/ProblemDetails, not unhandled 500.
- Verify errors/logs never echo passwords, JWT secrets, connection strings or SRI certificate passwords.
- Cross-channel People isolation: a Reservations request for community A must reject a requester belonging to community B, even when a client submits the GUID directly.
- Cross-channel Security isolation: a visit in community A must reject a host person belonging to community B, even when a client bypasses the UI selector.
- Verify Admin-created access grants remain scoped when later consumed by Resident, Supplier and Guard channels; Portal identity remains the authenticated authority.

## 22. Release smoke and rollback
- Run `scripts/production/validate-release.ps1` before deployment.
- Run `scripts/database/migration-precheck.ps1`; migration inventory precheck must enumerate all 16 approved DbContexts and at least one committed migration per context without connecting to SQL.
- Take/verify restorable DB backup.
- Run `scripts/database/backup-restore-smoke.ps1` in CI/non-production and retain PASS evidence proving backup, drop, restore and marker verification; this does not substitute target-environment backup evidence.
- Verify migration apply refuses execution without `-Apply`, without existing backup evidence, or without explicit `ConnectionStrings__<Module>` values for selected contexts.
- Apply migrations in TEST/UAT from same release SHA.
- Deploy immutable images.
- Run `scripts/production/smoke-test.ps1`. In an integrated environment readiness must return 200. In isolated CI, use `-ReadyExpectedStatus 503` to prove the application remains live while readiness fails closed when Portal is intentionally absent.
- CI runtime smoke must start the Docker Compose stack, verify API root/live health, verify unauthenticated session returns 401, verify Web responds, and always tear the stack down after the check.
- Record image digests and commit SHA via `scripts/production/capture-image-evidence.ps1`; CI must publish `artifacts/release` as a workflow artifact after backup/restore and runtime smoke pass.
- Run `scripts/production/rollback-rehearsal.ps1` in CI/non-production: stop the current ephemeral stack, build/deploy the previous immutable commit, rerun fail-closed smoke and retain rollback evidence.
- Run `scripts/database/migration-recovery-rehearsal.ps1` in CI/non-production: apply a real bounded-context migration set to an ephemeral database, observe a controlled transactional migration failure without partial schema residue, verify a forward-fix marker, then restore the real pre-migration backup and verify the baseline state.

## Exit criteria
A release candidate is operationally acceptable when automated CI is green, all applicable manual critical journeys above pass, no unresolved Critical/High security defect exists, backup/rollback evidence exists, and every external dependency required by the intended production scope is READY rather than BLOCKED.

## 23. Docker Hub runtime portability
- Populate `APPCONDOMINIO_API_IMAGE`, `APPCONDOMINIO_WORKER_IMAGE` and `APPCONDOMINIO_WEB_IMAGE` with immutable `@sha256:` references from the approved Docker Hub repositories.
- Run `scripts/local/start-dockerhub.ps1`; it must pull the three application images, validate the Compose overlay and start the stack with `--no-build`.
- Confirm API, Worker and Web are running from the pulled Hub digests, not from locally built `appcondominio-*` images.
- Re-run release smoke using ports resolved from `.env`; isolated environments expect readiness 503 until required Portal capabilities are available.
- Deleting local AppCondominio application images and repeating pull + startup must produce the same healthy runtime behavior.

## 24. Multi-workstation Docker Hub portability
- Validate the Docker Hub runtime independently on every workstation that will host the stack; do not treat another workstation's `localhost` as reachable locally.
- On Windows hosts with restrictive PowerShell policy, launch via `scripts/local/start-dockerhub.cmd` and verify that machine policy is not modified.
- With valid Docker Hub authentication, execute pull-only startup and confirm API root 200, live 200, isolated readiness 503, unauthenticated session 401 and Web 200 on each workstation.
- Confirm API, Worker and Web resolve to the approved immutable `@sha256:` references and are not local build tags.

## 25. UI-15 isolated dashboard acceptance
- With API reachable and no Portal session, open /dashboard; runtime/API status must remain visible and the page must not appear blank.
- Verify /api/ and /api/health/live are queried and displayed independently from authentication state.
- A 401 from /api/session must render Sin sesión / Portal guidance and must not be treated as a runtime failure.
- Protected /api/communities/dashboard metrics must only be requested after an authenticated session is confirmed; 401/403 remain fail-closed.
- Rebuild Angular production bundle and verify the existing authenticated dashboard behavior remains available when Portal supplies a valid session.

## 26. UI-16 Guard Portal guided workflow acceptance
- Guard visit search must match visitor name, document, destination and recorded gate/access.
- Authorized visits awaiting entry must require selection from the supported gate list before check-in can be submitted.
- After a successful check-in, the selected gate state must be cleared before the next operation.
- Incident reporting must use guided type/location selectors and reject blank or whitespace-only descriptions before calling the API.
- Existing Guard Portal 401/403 behavior, community isolation and backend contracts must remain unchanged.

## 27. UI-17 Supplier Portal bid workflow acceptance
- Bid submission must require an open process, amount greater than zero, integer delivery days greater than or equal to zero, and a non-blank proposal reference.
- The selected sourcing round closing time must remain visible while preparing the offer.
- Proposal reference must be trimmed before submission and invalid input must be rejected before calling the API.
- After a successful bid, amount/delivery/reference and selected process state must be reset before the next offer.
- Existing Supplier Portal 401/403 behavior, supplier isolation and backend contracts must remain unchanged.

## 28. UI-18 Resident Portal guided self-service acceptance
- Reservation submission must remain disabled until an authorized area is selected, start/end timestamps are valid with end after start, and guest count is an integer within 1..area capacity.
- Invalid reservation ranges or guest counts must be rejected in the UI before any API call is made.
- Visit authorization must require non-blank visitor name, document and destination plus a valid end-after-start authorization window.
- Visitor text fields must be trimmed before POST; successful reservation and visit submissions must clear their respective forms while keeping the selected resident unit.
- Existing Resident Portal 401/403 behavior, unit/community isolation and backend contracts must remain unchanged.

## 29. UI-19 Maintenance guided asset/KPI acceptance
- Asset creation must remain disabled until an authorized community and non-blank code, name, category and location are provided.
- Asset text fields must be trimmed before POST; successful creation must clear asset detail fields while preserving the selected community.
- KPI lookup must require an authorized community and must reject malformed or future as-of dates before calling the API.
- The date input must not allow selecting a date after today.
- Existing Maintenance 401/403 behavior, community isolation and backend contracts must remain unchanged.

## 30. UI-20 Reservations guided area/booking acceptance
- Area creation must require an authorized community, non-blank code/name, integer capacity >= 1 and non-negative finite fee before calling the API.
- Area code/name must be trimmed before POST; successful creation must clear area details while preserving the selected community and refresh booking options when applicable.
- Booking submission must require an authorized community, area and requester, a valid end-after-start interval and an integer guest count within 1..selected area capacity.
- Invalid date ranges or guest counts must be rejected in the UI before any API call is made.
- Existing Reservations 401/403 behavior and cross-community selector isolation must remain unchanged.

## 31. UI-21 Security visit/guard acceptance
- Visit authorization must require community, host, non-blank visitor/document/destination and a valid end-after-start validity range before calling the API.
- Visit free-text inputs must be trimmed before POST; successful authorization must clear visit details while preserving the selected community.
- Guard access must require community, non-blank Portal User Id/display name and, when provided, a valid future expiration before calling the API.
- Guard free-text inputs must be trimmed before POST; successful creation must preserve the selected community and clear user-specific fields.
- Existing Security 401/403 behavior, host selector isolation and KPI community scoping must remain unchanged.

## 32. UI-22 Billing guided monthly-period acceptance
- Monthly period requires an authorized community, integer year 2000..2100, month 1..12, issue date and due date.
- Due date cannot be earlier than issue date; invalid forms are rejected before the API call.
- Year/month are normalized to numeric values before POST.
- Successful creation preserves community/year/month and clears issue/due dates for the next operation.
- Statement lookup requires community + unit and unit options remain scoped to the selected community.
- Existing Billing 401/403 handling remains unchanged.

## 33. UI-23 Procurement guided requisition/supplier-access acceptance
- Requisition requires authorized community, nonblank number/description/requester and finite amount greater than zero.
- Requisition text values are trimmed before POST and community is preserved after success.
- Supplier access requires authorized community, supplier and nonblank Portal User Id.
- Optional supplier-access expiry must be a valid future date-time.
- Supplier/round selectors remain isolated by selected community.
- Existing Procurement 401/403 handling remains unchanged.
## 34. UI-24 Treasury guided payable acceptance
- Creating a payable requires an authorized community and an eligible supplier from that community.
- Document number, expense account, and payable account must contain non-whitespace text.
- Document date and due date must be valid dates, with due date on or after document date.
- Amount must be finite and greater than zero before the API call is attempted.
- Text values are trimmed and amount is normalized before POSTing the payable.
- After success, the selected community is preserved while payable details are reset and eligible suppliers are refreshed.
- Existing Treasury 401/403 behavior and community-scoped cash forecast remain unchanged.

## 35. UI-25 Collections payment/query acceptance
- Payment requires community, nonblank reference, allowed payment method, valid received date and finite amount > 0 before API call.
- Reference and optional external transaction id are trimmed before POST; blank external transaction becomes null.
- Successful payment preserves selected community and clears payment-specific fields.
- Receivables/aging require a community and reject an invalid optional cutoff date before API call.
- Community change clears unit/result state; unit options remain community-scoped.
- Existing Collections 401/403 behavior remains unchanged.

## 36. UI-26 Accounting guided period/trial-balance acceptance
- Opening a period requires an authorized community.
- Year must be an integer from 2000 through 2100.
- Month must be an integer from 1 through 12.
- Invalid period input is rejected before the API call.
- Year/month are normalized to numeric values before POST.
- Trial balance requires a selected community and remains community-scoped.
- Existing Accounting 401/403 behavior remains unchanged.

## 37. UI-27 Budgeting guided plan/variance acceptance
- Plan creation requires an authorized community, integer year 2000..2100, integer version >= 1 and trimmed non-empty name.
- Invalid plans are rejected client-side before the Budgeting API is called.
- Year/version are normalized to numbers and name is trimmed before POST.
- Successful creation preserves community/year/version, clears the plan name and reloads community-scoped plans.
- Variance lookup requires both community and plan context and clears stale variance when community changes.
- Existing Budgeting 401/403 behavior remains unchanged.

## 38. UI-28 SaaS commercial-plan acceptance
- Commercial plan requires nonblank code/name and exactly 3-letter currency.
- Unit/user/storage limits must be positive integers; price must be finite and nonnegative.
- Billing period is restricted to supported values 1, 3 or 12.
- At least one normalized module is required; duplicates and blanks are removed.
- Code/name/currency are trimmed and currency is uppercased before POST.
- Entitlements lookup requires an organization selection.
- Existing SaaS 401/403 handling remains unchanged.

## 39. UI-29 Supplier + Guard Portal final hardening acceptance
- Supplier bids require an existing selected round that is still open.
- Bid amount must be finite and greater than zero; delivery days must be a non-negative integer.
- Proposal reference is trimmed before POST; closed/invalid rounds cannot be selected for submission.
- Purchase-order acknowledge is allowed only for an existing order in Issued state.
- Guard check-in requires an authorized visit plus one configured gate from the allowed catalog.
- Guard check-out is allowed only for a visit currently inside the community.
- Guard incidents accept only configured type/location values and a nonblank trimmed description.
- Existing PortalCorporativo 401/403 behavior remains unchanged in both portals.
