# Conjunto al Día - Production Runbook

## Purpose
This runbook defines the minimum controlled process to promote AppCondominio from validated source to a production environment.

## Preconditions
- All required PRs in the release chain are reviewed and intentionally merged in order.
- CI for the release candidate is green.
- Production infrastructure, DNS/TLS, secrets, PortalCorporativo endpoints and SQL Server are defined outside the repository.
- Database backup/restore has been tested in the target environment.
- CI/non-production rehearsal via `scripts/database/backup-restore-smoke.ps1` proves SQL Server backup/restore mechanics, but does not replace target-environment backup evidence.
- Portal Security permissions/menu provisioning has been executed by an authorized operator.
- SRI provider/certificate configuration is available before enabling real electronic-document sending.

## Required production configuration
Never commit these values:
- SQL Server credentials/connection strings for each bounded context or approved shared database.
- Redis endpoint/credentials.
- RabbitMQ endpoint/virtual host/credentials.
- JWT validation secret or production signing-material equivalent supplied by Portal Security.
- PortalCorporativo gateway base URL.
- Seq/OpenTelemetry endpoints and credentials where applicable.
- SRI certificate, certificate password and provider credentials.

## Release sequence
1. Freeze the release candidate commit and record its SHA.
2. Run `scripts/production/validate-release.ps1` and `scripts/database/migration-precheck.ps1`; retain the generated migration inventory evidence.
3. Build immutable API, Worker and Web images from the release SHA; run `scripts/production/capture-image-evidence.ps1` and retain the generated content digests with the release evidence.
4. Back up all target SQL databases.
5. Apply EF Core migrations in a controlled maintenance window using `scripts/database/apply-migrations.ps1 -Apply -BackupEvidencePath <evidence>`; each selected bounded context requires its explicit `ConnectionStrings__<Module>` environment variable.
6. Provision/update Portal Security resources and menu definitions.
7. Deploy API and Worker.
8. Verify `/health/live`; then verify `/health/ready` after dependencies are reachable.
9. Deploy Web using the same release version.
10. Run `scripts/production/smoke-test.ps1` against the deployed endpoints.
11. Validate business-critical journeys from `docs/TEST_PLAN.md`.
12. Record release evidence: commit, image digests, migration result, smoke-test result and operator approvals.

## Migration policy
- Never auto-run destructive migrations during application startup.
- `migration-precheck.ps1` enumerates every approved DbContext without connecting to SQL and produces release evidence.
- `apply-migrations.ps1` is fail-closed: it requires `-Apply`, existing backup evidence and an explicit connection string for every selected context; it never uses cross-module connection-string fallbacks.
- Database migration is an explicit deployment activity.
- Take a restorable backup before migration.
- Rehearse backup/drop/restore verification in non-production using `backup-restore-smoke.ps1`; retain its JSON evidence alongside the release SHA.
- Validate schema changes in TEST/UAT before production.
- Financial/legal data is corrected through reversals/versioning, not destructive deletion.

## Rollback
Application rollback:
1. Stop new traffic or put gateway in maintenance mode.
2. Redeploy the previous immutable image set.
3. Re-run liveness/readiness checks. In non-production, rehearse this path with `scripts/production/rollback-rehearsal.ps1` and retain its JSON evidence.

Database rollback:
- Prefer forward-fix migrations.
- Restore from the pre-release backup only when the migration cannot be safely forward-fixed and business owners approve data-loss implications. Rehearse both paths in non-production with `scripts/database/migration-recovery-rehearsal.ps1`; the rehearsal uses only an ephemeral probe database.
- Never execute ad-hoc table deletion to undo a release.

## Health interpretation
- `/health/live`: process is running; does not imply dependency availability.
- `/health/ready`: SQL bounded contexts, Redis, RabbitMQ and Portal gateway are reachable.
- A failed readiness probe blocks production traffic but should not trigger destructive restart loops.

## Incident minimum evidence
Capture correlation ID, UTC timestamp, tenant/community identifiers available at the domain boundary, endpoint, HTTP status, application logs, dependency health and release SHA. Never copy production secrets or raw credentials into tickets/logs.

## Known external blockers
The application is intentionally fail-closed or partially blocked for Portal capabilities whose trusted contracts are not yet production-ready: tenant/community identity resolution, Audit ingestion identity, Notification tenant/service identity, Content/File API, Catalog API and tenant-aware Configuration. See the ADRs and `docs/DEPENDENCY_MATRIX.md`.

## Docker Hub deployment mode
- Publish API, Worker and Web images from the approved release SHA to Docker Hub and record immutable `@sha256:` references.
- Set the three `APPCONDOMINIO_*_IMAGE` variables outside Git and validate them with `scripts/production/validate-release.ps1`.
- Use `docker-compose.hub.yml` together with `docker-compose.yml`; application services must have local builds disabled by the overlay.
- Start with `scripts/local/start-dockerhub.ps1`, which performs pull, Compose validation, `--no-build` startup, service-state verification and smoke testing.
- Local deletion of AppCondominio application images is a supported portability check; the next startup must pull the exact approved digests from Docker Hub.
- Base infrastructure images may remain local or be pulled normally; this mode specifically guarantees API/Worker/Web provenance from Docker Hub.