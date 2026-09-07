using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppCondominio.Modules.Organizations.Infrastructure;

internal sealed class EfOrganizationRepository(OrganizationsDbContext dbContext)
    : IOrganizationRepository
{
    public async Task AddAsync(Organization organization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(organization);

        await dbContext.Organizations.AddAsync(organization, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Organizations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
