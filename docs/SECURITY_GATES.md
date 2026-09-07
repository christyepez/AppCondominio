# Security and quality gates

Sprint 1 establishes mandatory CI gates for AppCondominio.

## Required before merge

- .NET 10 restore and Release build.
- Unit tests.
- Architecture dependency tests.
- SQL Server integration tests using disposable Testcontainers infrastructure.
- Angular 18 production build.
- Docker image build for API, Worker and Web.
- Secret scanning with Gitleaks.
- Dependency review for high-severity additions.
- Dependabot for NuGet, npm, Docker and GitHub Actions.

## Deferred to later hardening

- Full SAST policy tuning.
- DAST against deployed environments.
- SBOM signing/attestation.
- Container registry admission policies.
- Production penetration testing.

No gate may be disabled simply to merge a failing story. A failure must be fixed or explicitly documented as a blocked dependency.
