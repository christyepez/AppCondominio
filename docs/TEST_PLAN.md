# Conjunto al DÃ­a - Product Test Plan

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
- Take/verify restorable DB backup.
- Apply migrations in TEST/UAT from same release SHA.
- Deploy immutable images.
- Run `scripts/production/smoke-test.ps1`.
- Record image digests and commit SHA.
- Test rollback to previous image set in non-production.
- Verify forward-fix/restore procedure for migration failure.

## Exit criteria
A release candidate is operationally acceptable when automated CI is green, all applicable manual critical journeys above pass, no unresolved Critical/High security defect exists, backup/rollback evidence exists, and every external dependency required by the intended production scope is READY rather than BLOCKED.
