using AppCondominio.Modules.Organizations.Application.Saas;
using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppCondominio.Modules.Organizations.Infrastructure;

internal sealed class EfSaasMetricsRepository(OrganizationsDbContext dbContext) : ISaasMetricsRepository
{
    public async Task<IReadOnlyCollection<Organization>> ListOrganizationsAsync(CancellationToken ct) =>
        await dbContext.Organizations.AsNoTracking().ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<Subscription>> ListSubscriptionsAsync(CancellationToken ct) =>
        await dbContext.Subscriptions.AsNoTracking().ToArrayAsync(ct);

    public async Task<IReadOnlyCollection<CommercialPlan>> ListPlansAsync(CancellationToken ct) =>
        await dbContext.CommercialPlans.AsNoTracking().ToArrayAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}
