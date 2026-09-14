using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.CreateOrganization;

public sealed class CreateOrganizationHandler(IOrganizationRepository repository)
{
    public async Task<CreateOrganizationResult> HandleAsync(
        CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        Organization organization = Organization.Create(command.Name, command.TaxId);
        await repository.AddAsync(organization, cancellationToken);

        return new CreateOrganizationResult(
            organization.Id,
            organization.Name,
            organization.TaxId,
            organization.IsActive);
    }
}
