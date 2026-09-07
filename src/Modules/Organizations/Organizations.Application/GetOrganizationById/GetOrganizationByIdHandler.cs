using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.GetOrganizationById;

public sealed class GetOrganizationByIdHandler(IOrganizationRepository repository)
{
    public async Task<GetOrganizationByIdResult?> HandleAsync(
        GetOrganizationByIdQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        Organization? organization = await repository.GetByIdAsync(query.Id, cancellationToken);
        return organization is null
            ? null
            : new GetOrganizationByIdResult(
                organization.Id,
                organization.Name,
                organization.TaxId,
                organization.IsActive);
    }
}
