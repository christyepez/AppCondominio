using System.Collections.Concurrent;
using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Infrastructure;

internal sealed class InMemoryOrganizationRepository : IOrganizationRepository
{
    private readonly ConcurrentDictionary<Guid, Organization> _organizations = new();

    public Task AddAsync(Organization organization, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_organizations.TryAdd(organization.Id, organization))
            throw new InvalidOperationException($"Organization '{organization.Id}' already exists.");
        return Task.CompletedTask;
    }

    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _organizations.TryGetValue(id, out Organization? organization);
        return Task.FromResult(organization);
    }
    public Task<IReadOnlyCollection<Organization>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Organization> organizations = _organizations.Values.OrderBy(x => x.Name).ToArray();
        return Task.FromResult(organizations);
    }
}
