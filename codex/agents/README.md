# AppCondominio local domain agents

Common architecture, security, QA, DevOps, Portal reuse and coordination agents live in `christyepez/CodexCommonAgents` and must not be duplicated here.

This directory is reserved only for condominium-domain specializations that add business knowledge on top of common agents.

Planned local agents:

1. `10-condominium-domain-agent.md` - communities, properties, ownership and people.
2. `11-billing-collections-agent.md` - fees, charges, receivables, payments and collections.
3. `12-accounting-treasury-agent.md` - accounting, AP, treasury and budget controls.
4. `13-ecuador-tax-agent.md` - Ecuador-specific tax/SRI domain rules and Portal integration boundaries.
5. `14-operations-agent.md` - procurement, maintenance, reservations and security operations.
6. `15-governance-agent.md` - assemblies, voting, infractions and coexistence processes.

Create a local agent only when its domain rules cannot be represented adequately by the existing common agents. Local agents inherit all rules from the pinned CodexCommonAgents revision.
