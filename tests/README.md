# Test structure

Target structure:

```text
tests/
├── AppCondominio.UnitTests/
├── AppCondominio.IntegrationTests/
├── AppCondominio.ArchitectureTests/
├── AppCondominio.ContractTests/
└── AppCondominio.EndToEndTests/
```

Mandatory coverage areas from the first executable slice:

- Domain invariants and value objects.
- FluentValidation rules.
- Vertical-slice handlers.
- SQL Server integration with isolated test database/Testcontainers.
- Architecture dependency rules.
- Tenant isolation when tenant-owned entities are introduced.
- Portal adapter contracts.
- Critical end-to-end flows with Playwright once the Angular shell is available.
