# Infrastructure structure

Target repository structure:

```text
infrastructure/
├── docker/
│   ├── api/
│   ├── worker/
│   └── web/
├── compose/
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── .env.example
├── sql/
│   ├── init/
│   └── diagnostics/
└── observability/
    ├── seq/
    └── otel/

deploy/
├── dev/
├── test/
├── prod/
└── runbooks/

scripts/
├── database/
├── docker/
├── ci/
└── codex/
```

Initial local services:

- appcondominio-api
- appcondominio-worker
- appcondominio-web
- sqlserver
- redis
- rabbitmq
- seq

PortalCorporativo is consumed as an external/shared platform and is not copied into this repository.
