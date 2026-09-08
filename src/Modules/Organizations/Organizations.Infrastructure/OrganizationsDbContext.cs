using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppCondominio.Modules.Organizations.Infrastructure;

public sealed class OrganizationsDbContext(DbContextOptions<OrganizationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<CommercialPlan> CommercialPlans => Set<CommercialPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<BrandSettings> BrandSettings => Set<BrandSettings>();
    public DbSet<TenantDatabaseProfile> TenantDatabaseProfiles => Set<TenantDatabaseProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationsDbContext).Assembly);
    }
}
