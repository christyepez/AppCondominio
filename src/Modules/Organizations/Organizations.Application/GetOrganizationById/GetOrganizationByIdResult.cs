namespace AppCondominio.Modules.Organizations.Application.GetOrganizationById;

public sealed record GetOrganizationByIdResult(Guid Id, string Name, string? TaxId, bool IsActive);
