using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.People.Infrastructure;

internal sealed class EfResidentAccessRepository(PeopleDbContext db) : IResidentAccessRepository
{
    public async Task<IReadOnlyCollection<AccessGrant>> ListActiveByExternalUserAsync(string externalUserId, CancellationToken cancellationToken) =>
        await db.AccessGrants
            .AsNoTracking()
            .Where(x => x.ExternalUserId == externalUserId && x.Status == AccessGrantStatus.Active)
            .OrderBy(x => x.CommunityId)
            .ThenBy(x => x.UnitId)
            .ToArrayAsync(cancellationToken);

    public Task<Person?> GetPersonAsync(Guid personId, CancellationToken cancellationToken) =>
        db.People.AsNoTracking().FirstOrDefaultAsync(x => x.Id == personId, cancellationToken);
}

public static class ResidentAccessDependencyInjection
{
    public static IServiceCollection AddResidentAccessContext(this IServiceCollection services)
    {
        services.AddScoped<IResidentAccessRepository, EfResidentAccessRepository>();
        services.AddScoped<ResidentAccessService>();
        return services;
    }
}
