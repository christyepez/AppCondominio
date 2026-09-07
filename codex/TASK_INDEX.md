# AppCondominio - Codex Task Index

## Sprint 01 - Architecture and foundation

| Order | Task | Story | Classification | Responsible agent | Status | Depends on |
|---:|---|---|---|---|---|---|
| 1 | Repository standards and .NET 10 solution skeleton | S01-01 | CREATE | Coordinator + Solution Architect | IMPLEMENTED / VALIDATION PENDING | - |
| 2 | Backend host and modular bootstrapper (.NET 10) | S01-02 | CREATE | Solution Architect + Backend Agent | IMPLEMENTED / VALIDATION PENDING | S01-01 |
| 3 | Organizations reference module | S01-03 | CREATE | Domain + Backend Agent | IMPLEMENTED / VALIDATION PENDING | S01-02 |
| 4 | SQL Server + EF Core 10 persistence and migrations | S01-04 | CREATE | Data + Backend Agent | IMPLEMENTED / VALIDATION PENDING | S01-03 |
| 5 | Angular shell integration skeleton | S01-05 | REUSE/EXTEND | Portal Reuse + Frontend Agent | IN PROGRESS | S01-01 |
| 6 | Docker Compose development topology | S01-06 | CREATE/REUSE | DevOps Agent | PLANNED | S01-02,S01-05 |
| 7 | Redis, RabbitMQ and Worker foundation | S01-07 | EXTEND/CREATE | Portal Reuse + Backend + DevOps | PLANNED | S01-02,S01-06 |
| 8 | Error handling, logging and observability | S01-08 | EXTEND/ADAPT | Architecture + Security/Observability | PLANNED | S01-02,S01-06 |
| 9 | Automated test foundation | S01-09 | CREATE | QA Agent | PLANNED | S01-03,S01-04 |
| 10 | GitHub Actions and security gates | S01-10 | CREATE | DevOps + Security + QA | PLANNED | S01-06,S01-09 |

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

## Validation note

The current ChatGPT execution environment does not provide the .NET SDK, so S01-01 through S01-04 are implemented but remain validation-pending until CI or a .NET 10-capable runner executes restore/build/tests/migrations. No build success is claimed yet.

## Execution rule

Do not start a task whose required dependency is not merged or explicitly accepted as a stable base. Each completed task must record branch, base commit, final commit, validations and next step.
