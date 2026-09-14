# Frontend structure

Target Angular structure, aligned with PortalCorporativo:

```text
frontend/
└── app-condominio-web/
    ├── src/app/
    │   ├── core/
    │   ├── shared/
    │   ├── layout/
    │   └── features/
    │       ├── organizations/
    │       ├── communities/
    │       ├── properties/
    │       ├── people/
    │       ├── billing/
    │       ├── collections/
    │       ├── accounting/
    │       ├── treasury/
    │       ├── budgeting/
    │       ├── procurement/
    │       ├── maintenance/
    │       ├── reservations/
    │       ├── security-operations/
    │       └── governance/
    ├── Dockerfile
    └── nginx.conf
```

The visual shell, menus, permissions and global configuration must reuse/extend PortalCorporativo rather than creating a second global shell implementation.
