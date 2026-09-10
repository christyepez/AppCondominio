# AGENTS.md - AppCondominio

## Common governance

This repository uses `christyepez/CodexCommonAgents` as its mandatory common governance baseline.

Before Docker, Docker Compose, image publishing, cleanup or multi-machine runtime work, Codex must read and apply:

```text
CodexCommonAgents/AGENTS.md
CodexCommonAgents/rules/02-docker-runtime-and-image-governance.md
CodexCommonAgents/playbooks/docker-multi-machine-runtime.md
```

## Project runtime rule

`trabajo` and `MarketingIndo` must use the same immutable project-owned image version when they represent the same AppCondominio environment.

The preferred runtime pattern is registry-based execution using immutable tags or image digests. The base Compose file defines topology; registry overrides replace project-owned `build:` entries with `image:` references.

Database and persistent volumes must be preserved during runtime alignment unless explicitly authorized otherwise.

## Validation

Docker changes must finish with `docker compose config`, image/digest comparison and health/port verification.
