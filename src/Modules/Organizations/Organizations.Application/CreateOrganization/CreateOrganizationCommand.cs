namespace AppCondominio.Modules.Organizations.Application.CreateOrganization;

public sealed record CreateOrganizationCommand(string Name, string? TaxId);
