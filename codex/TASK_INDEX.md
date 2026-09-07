# AppCondominio - Codex Task Index

## Sprint 01 - Architecture and foundation

| Order | Task | Story | Classification | Responsible agent | Status | Depends on |
|---:|---|---|---|---|---|---|
| 1 | Repository standards and .NET 10 solution skeleton | S01-01 | CREATE | Coordinator + Solution Architect | VALIDATED | - |
| 2 | Backend host and modular bootstrapper (.NET 10) | S01-02 | CREATE | Solution Architect + Backend Agent | VALIDATED | S01-01 |
| 3 | Organizations reference module | S01-03 | CREATE | Domain + Backend Agent | VALIDATED | S01-02 |
| 4 | SQL Server + EF Core 10 persistence and migrations | S01-04 | CREATE | Data + Backend Agent | VALIDATED | S01-03 |
| 5 | Angular 21 LTS shell integration skeleton | S01-05 | REUSE/EXTEND | Portal Reuse + Frontend Agent | VALIDATED | S01-01 |
| 6 | Docker Compose development topology | S01-06 | CREATE/REUSE | DevOps Agent | VALIDATED | S01-02,S01-05 |
| 7 | Redis, RabbitMQ and Worker foundation | S01-07 | EXTEND/CREATE | Portal Reuse + Backend + DevOps | VALIDATED | S01-02,S01-06 |
| 8 | Error handling, logging and observability | S01-08 | EXTEND/ADAPT | Architecture + Security/Observability | VALIDATED | S01-02,S01-06 |
| 9 | Automated test foundation | S01-09 | CREATE | QA Agent | VALIDATED | S01-03,S01-04 |
| 10 | GitHub Actions and security gates | S01-10 | CREATE | DevOps + Security + QA | VALIDATED | S01-06,S01-09 |

## Sprint 01 validation evidence

Final validated CI run: `34161344428`.

Passed gates:

- Gitleaks secret scan.
- .NET 10 restore.
- NuGet direct/transitive vulnerability scan.
- .NET 10 Release build.
- Unit tests.
- Architecture tests.
- SQL Server/Testcontainers integration tests and EF migration execution.
- npm high/critical vulnerability scan.
- Angular 21 LTS production build.
- Docker builds for API, Worker and Web.

Security decision: Angular 18 alignment with the current PortalCorporativo frontend was rejected after CI found 58 vulnerabilities (32 high, 1 critical). AppCondominio uses Angular 21 LTS while preserving Portal integration through APIs/contracts/configuration. See `docs/adr/ADR-001-angular-security-baseline.md`.

## Sprint 02 - PortalCorporativo integration

| Order | Task | Story | Classification | Status |
|---:|---|---|---|---|
| 1 | Authentication/JWT integration | S02-01 | REUSE/ADAPT | PLANNED |
| 2 | User/Tenant/Community context | S02-02 | ADAPT/CREATE | PLANNED |
| 3 | Permission authorization | S02-03 | REUSE/EXTEND | PLANNED |
| 4 | Menu registration | S02-04 | EXTEND | PLANNED |
| 5 | Audit integration | S02-05 | ADAPT | PLANNED |
| 6 | Notification integration | S02-06 | ADAPT | PLANNED |
| 7 | Content/File integration | S02-07 | ADAPT | PLANNED |
| 8 | Catalog integration | S02-08 | EXTEND/ADAPT | PLANNED |
| 9 | Configuration integration | S02-09 | EXTEND/ADAPT | PLANNED |
| 10 | Distributed health and resilience | S02-10 | ADAPT | PLANNED |

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
