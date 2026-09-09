using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.ListOrganizations;

public sealed class ListOrganizationsHandler(IOrganizationRepository repository)
{
    public async Task<IReadOnlyCollection<ListOrganizationsResult>> HandleAsync(
        ListOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyCollection<Organization> organizations = await repository.ListAsync(cancellationToken);
        return organizations
            .Where(x => !query.ActiveOnly || x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ListOrganizationsResult(x.Id, x.Name, x.TaxId, x.IsActive))
            .ToArray();
    }
}
