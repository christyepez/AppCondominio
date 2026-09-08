using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Billing.Infrastructure;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options):DbContext(options)
{
    public DbSet<ChargeConcept> Concepts=>Set<ChargeConcept>();
    public DbSet<ChargeConceptVersion> Versions=>Set<ChargeConceptVersion>();
    public DbSet<AccountingMapping> AccountingMappings=>Set<AccountingMapping>();
    public DbSet<InterestRule> InterestRules=>Set<InterestRule>();
    public DbSet<DiscountRule> DiscountRules=>Set<DiscountRule>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
}

internal sealed class EfBillingRepository(BillingDbContext db):IBillingRepository
{
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task<ChargeConcept?> GetConceptAsync(Guid id,CancellationToken ct)=>db.Concepts.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<ChargeConceptVersion?> GetVersionAsync(Guid id,CancellationToken ct)=>db.Versions.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<DiscountRule?> GetDiscountAsync(Guid communityId,Guid conceptId,DateOnly on,CancellationToken ct)=>db.DiscountRules.Where(x=>x.CommunityId==communityId&&x.ConceptId==conceptId&&x.ValidFrom<=on&&(x.ValidTo==null||x.ValidTo>=on)).OrderByDescending(x=>x.ValidFrom).FirstOrDefaultAsync(ct);
    public Task<InterestRule?> GetInterestAsync(Guid communityId,Guid conceptId,CancellationToken ct)=>db.InterestRules.Where(x=>x.CommunityId==communityId&&x.ConceptId==conceptId).OrderByDescending(x=>x.Id).FirstOrDefaultAsync(ct);
    public async Task<int> NextVersionAsync(Guid communityId,Guid conceptId,CancellationToken ct)=>(await db.Versions.Where(x=>x.CommunityId==communityId&&x.ConceptId==conceptId).MaxAsync(x=>(int?)x.Version,ct)??0)+1;
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class BillingMap
{
    public static void Apply<T>(EntityTypeBuilder<T> b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"billing");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}
}
internal sealed class ConceptConfig:IEntityTypeConfiguration<ChargeConcept>{public void Configure(EntityTypeBuilder<ChargeConcept>b){BillingMap.Apply(b,"ChargeConcepts");b.Property(x=>x.Code).HasMaxLength(80).IsRequired();b.Property(x=>x.Name).HasMaxLength(200).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.Code}).IsUnique();}}
internal sealed class VersionConfig:IEntityTypeConfiguration<ChargeConceptVersion>{public void Configure(EntityTypeBuilder<ChargeConceptVersion>b){BillingMap.Apply(b,"ChargeConceptVersions");b.Property(x=>x.FixedAmount).HasPrecision(18,6);b.Property(x=>x.Rate).HasPrecision(18,6);b.Property(x=>x.Minimum).HasPrecision(18,6);b.Property(x=>x.Maximum).HasPrecision(18,6);b.Property(x=>x.ApprovedBy).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.ConceptId,x.Version}).IsUnique();}}
internal sealed class AccountingConfig:IEntityTypeConfiguration<AccountingMapping>{public void Configure(EntityTypeBuilder<AccountingMapping>b){BillingMap.Apply(b,"AccountingMappings");b.Property(x=>x.RevenueAccount).HasMaxLength(80).IsRequired();b.Property(x=>x.ReceivableAccount).HasMaxLength(80).IsRequired();b.Property(x=>x.TaxAccount).HasMaxLength(80);b.Property(x=>x.CostCenter).HasMaxLength(80);b.HasIndex(x=>new{x.CommunityId,x.ConceptId});}}
internal sealed class InterestConfig:IEntityTypeConfiguration<InterestRule>{public void Configure(EntityTypeBuilder<InterestRule>b){BillingMap.Apply(b,"InterestRules");b.Property(x=>x.Percentage).HasPrecision(18,6);b.Property(x=>x.FixedAmount).HasPrecision(18,6);b.Property(x=>x.MaximumAmount).HasPrecision(18,6);b.HasIndex(x=>new{x.CommunityId,x.ConceptId});}}
internal sealed class DiscountConfig:IEntityTypeConfiguration<DiscountRule>{public void Configure(EntityTypeBuilder<DiscountRule>b){BillingMap.Apply(b,"DiscountRules");b.Property(x=>x.Percentage).HasPrecision(18,6);b.Property(x=>x.FixedAmount).HasPrecision(18,6);b.HasIndex(x=>new{x.CommunityId,x.ConceptId,x.ValidFrom});}}

public static class DependencyInjection
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services,IConfiguration configuration)
    {
        string cs=configuration.GetConnectionString("Billing")??configuration.GetConnectionString("People")??configuration.GetConnectionString("Properties")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Billing connection string is required.");
        services.AddDbContext<BillingDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","billing")));
        services.AddScoped<IBillingRepository,EfBillingRepository>();services.AddScoped<BillingService>();return services;
    }
}
