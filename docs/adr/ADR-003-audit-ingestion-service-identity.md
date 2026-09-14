# ADR-003 - Audit Ingestion Requires Trusted Service Identity

## Status

Accepted as a blocking security decision for Sprint 2 story S02-05.

## Context

AppCondominio must reuse PortalCorporativo Audit API rather than create a parallel audit engine.

The current Portal contract accepts:

`CreateAuditEventRequest(string ActorId, string? TenantId, string Resource, string Action, string EntityName, string? EntityId, string? BeforeJson, string? AfterJson, string? MetadataJson, string? CorrelationId, string? CausationId, string? RequestId, string? IpAddress, string? UserAgent, int Severity)`.

The write endpoint is:

`POST /api/audit/events`

and requires the transverse permission `portal.audit.write`.

The current Audit API trusts `ActorId` from the request body. It does not derive or verify the actor against the authenticated JWT principal. It also normalizes a null tenant to `default`.

PortalCorporativo currently has no documented workload/service-to-service identity contract for domain APIs such as AppCondominio.

## Security risk

Forwarding the end-user JWT to Audit API would require ordinary AppCondominio users to hold `portal.audit.write`. A user possessing that token could potentially call Audit API directly and submit a different `ActorId`, producing a forged audit trail.

Using a static bearer token in AppCondominio configuration would replace one problem with another: long-lived application credentials without a defined rotation/workload identity contract.

Because audit records are security evidence, a best-effort or spoofable transport is unacceptable.

## Decision

S02-05 transport integration is BLOCKED until PortalCorporativo provides a trusted audit-ingestion authentication contract.

AppCondominio will NOT:

- grant `portal.audit.write` to ordinary residents or application users solely so domain APIs can write audit events;
- forward an end-user token to an API that trusts an arbitrary body `ActorId`;
- store a hard-coded or committed Portal bearer token;
- write directly to the PortalAudit database;
- create a local competing audit database/engine;
- silently drop required audit events when the remote Audit API is unavailable.

## Approved future options

Preferred order:

1. PortalCorporativo introduces workload/service identity for trusted domain applications and Audit API accepts only trusted application callers for ingestion.
2. Portal Audit adds a dedicated ingestion contract where caller identity is authenticated as AppCondominio and actor/tenant metadata is validated against signed/request context.
3. If Portal standardizes delegated user ingestion, Audit API must derive `ActorId` from a documented signed claim rather than trusting the request body, and tenant context must also be trusted.

The chosen contract must define token acquisition, rotation, audience, permissions/scopes, retry/idempotency behavior and network exposure.

## Application contract preparation

A future AppCondominio audit port may model domain intent independently of HTTP, for example actor, tenant, resource, action, entity, before/after metadata and correlation. Infrastructure will adapt that port to the approved Portal contract.

The port must not expose Portal-specific authentication details to domain/application code.

## Reliability requirement

For critical financial/legal writes, remote audit delivery should eventually use a reliable local outbox/integration mechanism rather than making domain transaction success depend on a synchronous network call. The exact event/transport contract must align with Portal's supported audit ingestion model.

## Consequences

- S02-05 is marked BLOCKED rather than implementing an unsafe HTTP adapter.
- S02-06 and other Portal integrations may continue if their own contracts can be consumed securely.
- PortalCorporativo requires an explicit security enhancement before production-grade audit ingestion can be completed.
