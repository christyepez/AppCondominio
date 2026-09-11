namespace AppCondominio.Modules.Organizations.Application.CreateOrganization;

public sealed record CreateOrganizationResult(Guid Id, string Name, string? TaxId, bool IsActive);
