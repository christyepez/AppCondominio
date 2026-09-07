# ADR-004 - Notification Transport Requires Trusted Service Identity and Tenant Context

## Status

Accepted as a blocking architecture/security decision for Sprint 2 story S02-06.

## Context

AppCondominio must reuse PortalCorporativo Notification API. The current Portal contracts support templates, immediate/scheduled sends, recipients, variables, channels and an idempotency key.

Runtime endpoints include:

- `POST /api/notifications/send` requiring `portal.notification.send`;
- `POST /api/notifications/schedule` requiring `portal.notification.send`.

The current Notification service uses a hard-coded tenant value:

`const string Tenant = "default";`

and `SendNotificationRequest` accepts an arbitrary array of recipients supplied by the caller.

PortalCorporativo currently has no documented workload/service-to-service identity contract for AppCondominio and no tenant-aware notification ingestion contract for domain applications.

## Security and multi-tenant risk

Using an end-user JWT for system-generated notifications would require granting ordinary application users the transverse `portal.notification.send` permission. A bearer with that permission could potentially call Notification API directly and choose arbitrary recipients/template parameters, bypassing AppCondominio domain authorization.

The hard-coded `default` tenant also prevents AppCondominio from proving isolation between condominium tenants or from applying tenant-specific templates/preferences safely.

## Decision

S02-06 runtime notification integration is BLOCKED until PortalCorporativo provides both:

1. a trusted workload/service identity or equivalent server-to-server caller contract; and
2. a tenant-aware notification contract that does not force all messages/templates into `default`.

AppCondominio will NOT:

- grant `portal.notification.send` to ordinary residents solely for backend-generated notifications;
- forward end-user tokens as the general-purpose system notification credential;
- store a static or committed Portal bearer token;
- duplicate Portal notification templates, retries, provider engines or message persistence;
- assume tenant `default` for production condominium notifications;
- accept arbitrary recipient lists without domain-side authorization/derivation.

## Future integration shape

Once Portal supplies the required contract, AppCondominio should expose an application port independent of HTTP. Domain/application code should express notification intent (template/purpose, authorized recipients, variables, tenant/community context and idempotency key), while Infrastructure adapts to Portal Notification API.

System-generated notification delivery should be initiated from reliable domain/integration events where appropriate so business transactions do not depend synchronously on provider/network availability.

## Template registration

No AppCondominio templates are registered in Sprint 2 because the product does not yet have concrete billing, collections, reservation or governance notification use cases. Templates will be added together with the bounded context that owns the business message instead of creating speculative global templates.

## Consequences

- S02-06 is marked BLOCKED rather than introducing an unsafe delegated-user transport.
- Existing Portal notification functionality remains the target reusable capability.
- Portal requires tenant-aware and workload-identity enhancement before production integration.
- S02-07 may proceed independently after inspecting Content/File API contracts.
