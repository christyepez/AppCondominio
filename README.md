# AppCondominio

SaaS multiempresa para la administracion integral de condominios y conjuntos residenciales.

## Arquitectura base

- .NET 8 / ASP.NET Core
- Angular, alineado al PortalCorporativo
- SQL Server
- Redis
- RabbitMQ
- Docker Compose
- Modular Monolith
- Clean Architecture
- Vertical Slice
- DDD
- CQRS pragmatica
- Outbox / eventos de dominio / eventos de integracion

## Reutilizacion transversal

Este repositorio usa `christyepez/CodexCommonAgents` como contrato comun de agentes y `christyepez/PortalCorporativo` como plataforma transversal.

Antes de crear una capacidad, se debe clasificar como `REUSE`, `EXTEND`, `ADAPT`, `CREATE` o `BLOCKED`.

## Estado

Linea base inicial para Sprint 1: arquitectura, repositorio y entorno de desarrollo.
