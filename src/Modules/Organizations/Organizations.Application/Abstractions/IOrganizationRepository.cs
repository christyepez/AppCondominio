using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.Abstractions;

public interface IOrganizationRepository
{
    Task AddAsync(Organization organization, CancellationToken cancellationToken);

    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
