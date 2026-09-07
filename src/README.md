# Backend source structure

Target structure for Sprint 1:

```text
src/
├── AppCondominio.Api/
├── AppCondominio.Bootstrapper/
├── AppCondominio.Contracts/
├── AppCondominio.SharedKernel/
├── AppCondominio.Infrastructure/
├── AppCondominio.Worker/
└── Modules/
    ├── Organizations/
    │   ├── AppCondominio.Modules.Organizations.Domain/
    │   ├── AppCondominio.Modules.Organizations.Application/
    │   ├── AppCondominio.Modules.Organizations.Infrastructure/
    │   └── AppCondominio.Modules.Organizations.Api/
    ├── Communities/
    ├── Properties/
    ├── Ownership/
    ├── People/
    ├── Billing/
    ├── Collections/
    ├── Accounting/
    ├── Treasury/
    ├── Budgeting/
    ├── Procurement/
    ├── Maintenance/
    ├── Reservations/
    ├── SecurityOperations/
    ├── Governance/
    ├── Tax/
    ├── Reporting/
    └── Integrations/
```

Only `Organizations` must be fully scaffolded as the reference module in the first foundation slice. Other modules should be added incrementally to avoid an empty-project explosion.

Each implemented module follows Domain -> Application -> Infrastructure/API dependencies and exposes explicit contracts/events for cross-module interactions.
