# ADR-005 - Portal Content/File Contract Is Required Before Domain Integration

## Status

Accepted as a blocking dependency decision for Sprint 2 story S02-07.

## Context

AppCondominio must reuse PortalCorporativo for generic file/content management rather than create a parallel cross-cutting file service.

The current `Portal.Content.Contracts` project contains only the placeholder `Class1.cs`. The current Content API bootstraps Portal foundation and exposes only:

`GET /` -> `{ service = "Portal.Content.Api", status = "bootstrap" }`

There are currently no reusable contracts/endpoints for upload, download, metadata, versioning, delete/retention, tenant ownership, authorization, file size/type validation or storage-provider abstraction.

## Decision

S02-07 is `BLOCKED` until PortalCorporativo implements and documents a functional Content/File API contract.

AppCondominio will NOT:

- implement a generic local file repository that duplicates the planned Portal capability;
- write directly to Portal content storage or database internals;
- invent Portal upload/download endpoints or DTOs;
- bind domain models to an assumed Blob/SharePoint/filesystem provider;
- accept client-supplied tenant/community ownership metadata as authoritative;
- create production document handling without explicit authorization and malware/content validation requirements.

## Minimum Portal contract required

A reusable Portal Content/File capability should define at least:

1. upload/initiate-upload contract;
2. download/read contract;
3. file metadata response and lookup;
4. tenant/owner/resource association semantics;
5. authorization model for read/write/delete/version operations;
6. maximum sizes and allowed content-type/extension policy;
7. storage-provider abstraction and external configuration;
8. versioning/replacement semantics;
9. retention/soft-delete/legal-hold behavior where applicable;
10. correlation/audit integration;
11. malware/content scanning strategy for untrusted uploads;
12. service identity or delegated-user rules for domain applications.

## AppCondominio future shape

Domain modules will reference local application ports representing business document intent, while Infrastructure adapts those ports to Portal Content/File contracts. Modules should store Portal content identifiers/metadata references, not provider-specific paths or credentials.

Examples of later domain use cases include invoices/RIDE/XML, supplier documents, maintenance evidence, assembly minutes and resident attachments. Those will be introduced with the bounded context that owns the document.

## Consequences

- S02-07 remains blocked without creating technical debt through a competing file engine.
- S02-08 Catalog integration may proceed independently after inspecting its actual Portal contracts.
- PortalCorporativo remains the intended owner of generic file/content functionality.
