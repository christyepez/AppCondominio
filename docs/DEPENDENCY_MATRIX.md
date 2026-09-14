# Conjunto al Día - External Dependency Matrix

| Capability | Owner | Current state | AppCondominio behavior | Production action |
|---|---|---|---|---|
| JWT validation | PortalCorporativo Security | VALIDATED contract | Validates issuer/audience/signature/lifetime and signed `permission` claims | Supply production signing configuration/secret outside Git |
| Tenant/User context | PortalCorporativo Security | BLOCKED | Does not invent tenant/community JWT claims; CommunityId remains explicit domain input | Portal must expose trusted self/service tenant context |
| Permission authorization | PortalCorporativo Security | VALIDATED contract | Uses `appcondominio.*.read/manage` policies | Provision security manifest |
| Menu | PortalCorporativo Menu | VALIDATED contract | Provisioning manifest/script available | Provision menu after Security resources |
| Audit ingestion | PortalCorporativo Audit | BLOCKED | No unsafe user-token forwarding | Add trusted service identity/actor enforcement |
| Notifications | PortalCorporativo Notification | BLOCKED | Emits integration events where applicable; does not grant arbitrary send privilege | Add tenant-aware service identity and routing |
| Content / Files | PortalCorporativo Content | BLOCKED | Stores references/metadata only where required; no parallel file service | Portal must expose upload/download/metadata contract |
| Catalog | PortalCorporativo Catalog | BLOCKED | Domain-owned catalogs remain local; no duplicate Portal catalog integration | Portal must expose functional catalog contract |
| Configuration | PortalCorporativo Configuration | BLOCKED for multi-tenant | Does not treat hard-coded `default` tenant as production tenant | Portal must derive trusted TenantId |
| Redis | Platform | READY by configuration | Distributed cache/readiness | Provision production endpoint/credentials |
| RabbitMQ | Platform | READY by configuration | Integration messaging/readiness | Provision production endpoint/vhost/credentials |
| SQL Server | Platform | READY by configuration | Independent schemas/DbContexts and migrations | Backup, migration precheck, HA/DR policy |
| Seq / OpenTelemetry | Platform | READY by configuration | Structured logs/traces | Configure retention, access and alerting |
| SRI electronic documents | Tax/provider | ADAPTER READY; external config pending | `PendingConfiguration` until real provider/certificate exists | Configure approved SRI provider, certificate and secrets; execute certification/UAT |

## Release rule
A capability marked BLOCKED must not be replaced by a local duplicate merely to pass production readiness. The release may proceed only if the business scope does not require that capability, or after the owning platform dependency is resolved and revalidated.
