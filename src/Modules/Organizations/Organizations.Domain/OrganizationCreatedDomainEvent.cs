using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Organizations.Domain;

public sealed record OrganizationCreatedDomainEvent(
    Guid EventId,
    Guid OrganizationId,
    DateTime OccurredAtUtc) : IDomainEvent;
