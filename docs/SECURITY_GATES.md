# Security and quality gates

Sprint 1 establishes mandatory CI gates for AppCondominio.

## Required before merge

- .NET 10 restore and Release build.
- NuGet direct/transitive vulnerability scan using `dotnet list ... package --vulnerable --include-transitive`.
- Unit tests.
- Architecture dependency tests.
- SQL Server integration tests using disposable Testcontainers infrastructure.
- `npm audit --audit-level=high`.
- Angular 21 LTS production build.
- Docker image build for API, Worker and Web.
- Secret scanning with Gitleaks.
- Dependabot for NuGet, npm, Docker and GitHub Actions.

## GitHub Dependency Review

GitHub Dependency Review was initially configured but the repository reported that Dependency Graph was not enabled, so the action could not operate. It was removed as a required gate and replaced by portable NuGet/npm vulnerability scans that work independently of that repository setting.

Dependency Review may be reintroduced later after Dependency Graph is enabled, as an additional control rather than a replacement for package-manager scans.

## Sprint 1 security finding

The initial Angular 18.2.x baseline inherited from PortalCorporativo failed the npm vulnerability gate with 58 findings: 6 low, 19 moderate, 32 high and 1 critical. The findings included Angular runtime security advisories as well as vulnerable build-tool dependencies.

AppCondominio was upgraded to Angular 21 LTS. The subsequent `npm audit --audit-level=high` and production build passed in CI run `34161344428`.

## Validated Sprint 1 run

CI run `34161344428` passed:

- Gitleaks
- NuGet vulnerability scan
- .NET 10 Release build
- unit tests
- architecture tests
- SQL Server/Testcontainers integration tests
- npm vulnerability scan
- Angular 21 production build
- Docker API/Worker/Web image builds

## Deferred to later hardening

- Full SAST policy tuning.
- DAST against deployed environments.
- SBOM signing/attestation.
- Container registry admission policies.
- Production penetration testing.

No gate may be disabled simply to merge a failing story. A failure must be fixed or explicitly documented as a blocked dependency.
