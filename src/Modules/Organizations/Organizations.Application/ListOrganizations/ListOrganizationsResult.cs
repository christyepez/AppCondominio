namespace AppCondominio.Modules.Organizations.Application.ListOrganizations;

public sealed record ListOrganizationsResult(Guid Id, string Name, string? TaxId, bool IsActive);
