using AppCondominio.Modules.Organizations.Application.Saas;
using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppCondominio.Modules.Organizations.Infrastructure;

internal sealed class EfSaasRepository(OrganizationsDbContext dbContext) : ISaasRepository
{
    public Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken ct) => dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<CommercialPlan?> GetPlanAsync(Guid id, CancellationToken ct) => dbContext.CommercialPlans.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyCollection<CommercialPlan>> ListPlansAsync(CancellationToken ct) => await dbContext.CommercialPlans.OrderBy(x => x.Name).ToArrayAsync(ct);
    public Task<Subscription?> GetSubscriptionByOrganizationAsync(Guid organizationId, CancellationToken ct) => dbContext.Subscriptions.FirstOrDefaultAsync(x => x.OrganizationId == organizationId, ct);
    public Task<BrandSettings?> GetBrandingAsync(Guid organizationId, CancellationToken ct) => dbContext.BrandSettings.FirstOrDefaultAsync(x => x.OrganizationId == organizationId, ct);
    public Task<TenantDatabaseProfile?> GetDatabaseProfileAsync(Guid organizationId, CancellationToken ct) => dbContext.TenantDatabaseProfiles.FirstOrDefaultAsync(x => x.OrganizationId == organizationId, ct);
    public Task AddPlanAsync(CommercialPlan plan, CancellationToken ct) => dbContext.CommercialPlans.AddAsync(plan, ct).AsTask();
    public Task AddSubscriptionAsync(Subscription subscription, CancellationToken ct) => dbContext.Subscriptions.AddAsync(subscription, ct).AsTask();
    public Task AddBrandingAsync(BrandSettings branding, CancellationToken ct) => dbContext.BrandSettings.AddAsync(branding, ct).AsTask();
    public Task AddDatabaseProfileAsync(TenantDatabaseProfile profile, CancellationToken ct) => dbContext.TenantDatabaseProfiles.AddAsync(profile, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct) => dbContext.SaveChangesAsync(ct);
}

internal sealed class CommercialPlanConfiguration : IEntityTypeConfiguration<CommercialPlan>
{
    public void Configure(EntityTypeBuilder<CommercialPlan> b)
    {
        b.ToTable("CommercialPlans", "organizations"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Code).HasMaxLength(80).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(160).IsRequired(); b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.Price).HasPrecision(18, 2); b.Property(x => x.ModulesCsv).HasMaxLength(2000).IsRequired();
        b.Ignore(x => x.Modules); b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("Subscriptions", "organizations"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.OrganizationId).IsUnique(); b.HasIndex(x => x.PlanId); b.Property(x => x.ModulesCsv).HasMaxLength(2000).IsRequired();
        b.Property(x => x.CommercialNotes).HasMaxLength(1000); b.Ignore(x => x.Modules); b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class BrandSettingsConfiguration : IEntityTypeConfiguration<BrandSettings>
{
    public void Configure(EntityTypeBuilder<BrandSettings> b)
    {
        b.ToTable("BrandSettings", "organizations"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.OrganizationId).IsUnique(); b.Property(x => x.ProductName).HasMaxLength(160).IsRequired();
        b.Property(x => x.LogoUrl).HasMaxLength(1000); b.Property(x => x.FaviconUrl).HasMaxLength(1000);
        b.Property(x => x.PrimaryColor).HasMaxLength(16).IsRequired(); b.Property(x => x.SecondaryColor).HasMaxLength(16).IsRequired();
        b.Property(x => x.ContactEmail).HasMaxLength(320); b.Property(x => x.ContactPhone).HasMaxLength(60); b.Ignore(x => x.DomainEvents);
    }
}

internal sealed class TenantDatabaseProfileConfiguration : IEntityTypeConfiguration<TenantDatabaseProfile>
{
    public void Configure(EntityTypeBuilder<TenantDatabaseProfile> b)
    {
        b.ToTable("TenantDatabaseProfiles", "organizations"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.OrganizationId).IsUnique(); b.Property(x => x.ConnectionSecretReference).HasMaxLength(500);
        b.Property(x => x.MigrationStatus).HasMaxLength(80).IsRequired(); b.Ignore(x => x.DomainEvents);
    }
}
