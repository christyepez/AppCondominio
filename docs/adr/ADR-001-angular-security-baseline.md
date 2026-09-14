# ADR-001 - Angular security baseline for AppCondominio

- Status: Accepted
- Date: 2026-09-07
- Scope: AppCondominio frontend

## Context

PortalCorporativo currently uses Angular 18.2.x. AppCondominio initially adopted the same Angular line to maximize implementation alignment.

During Sprint 1, the CI security gate executed `npm audit --audit-level=high` against that dependency graph and reported 58 vulnerabilities: 6 low, 19 moderate, 32 high and 1 critical. The findings included Angular runtime advisories involving XSS, information leakage and denial of service, in addition to vulnerable build-tool dependencies.

Keeping Angular 18 only to match PortalCorporativo would therefore make AppCondominio inherit known high/critical security exposure.

## Decision

AppCondominio uses Angular 21 LTS as its approved frontend baseline.

Current baseline:

```text
Angular runtime: 21.2.x
Angular CLI/build: 21.2.x
TypeScript: 5.9.x
```

PortalCorporativo integration remains mandatory, but compatibility is achieved through:

- APIs and explicit contracts;
- authentication/authorization integration;
- menu and configuration metadata;
- routing conventions;
- shared UX/design conventions where practical;
- reusable Portal services rather than copying Portal frontend source.

Identical Angular major versions are not an integration requirement.

## Consequences

Positive:

- AppCondominio does not knowingly introduce the Angular 18 high/critical findings detected by CI.
- Frontend security scanning becomes a release gate.
- Portal and domain application are less tightly coupled at framework level.

Trade-offs:

- PortalCorporativo and AppCondominio temporarily run different Angular major versions.
- Any future shared Angular package must declare compatibility explicitly rather than assuming identical runtime versions.
- PortalCorporativo should receive a separate upgrade/hardening initiative.

## Validation

GitHub Actions run `34161344428` passed:

- `npm audit --audit-level=high`;
- Angular 21 production build;
- Docker web image build;
- all backend, SQL integration, vulnerability and secret gates.

## Revisit criteria

Revisit this ADR only when:

1. PortalCorporativo moves to a supported secure Angular baseline and shared runtime alignment offers measurable value; or
2. Angular 21 reaches end of support or receives a security issue requiring a major-version change.

Any change requires an explicit architecture/security story and successful CI security/build validation.
