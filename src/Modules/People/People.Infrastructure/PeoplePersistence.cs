using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.People.Infrastructure;

public sealed class PeopleDbContext(DbContextOptions<PeopleDbContext> options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<Ownership> Ownerships => Set<Ownership>();
    public DbSet<LeaseContract> Leases => Set<LeaseContract>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<UnitAccessCode> AccessCodes => Set<UnitAccessCode>();
    public DbSet<ActivationRequest> Activations => Set<ActivationRequest>();
    public DbSet<AccessGrant> AccessGrants => Set<AccessGrant>();
    public DbSet<FinancialResponsibility> FinancialResponsibilities => Set<FinancialResponsibility>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(PeopleDbContext).Assembly);
}

internal sealed class EfPeopleRepository(PeopleDbContext db) : IPeopleRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class => await db.Set<T>().AddAsync(entity, cancellationToken);
    public Task<Person?> GetPersonAsync(Guid id, CancellationToken cancellationToken) => db.People.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<ActivationRequest?> GetActivationAsync(Guid id, CancellationToken cancellationToken) => db.Activations.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<UnitAccessCode?> FindUsableAccessCodeAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken) => db.AccessCodes.Where(x => x.CommunityId == communityId && x.UnitId == unitId && x.ConsumedAtUtc == null && x.ExpiresAtUtc >= DateTimeOffset.UtcNow).OrderByDescending(x => x.ExpiresAtUtc).FirstOrDefaultAsync(cancellationToken);
    public async Task<IReadOnlyCollection<Ownership>> ListOwnershipsAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken) => await db.Ownerships.Where(x => x.CommunityId == communityId && x.UnitId == unitId).OrderByDescending(x => x.StartsOn).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<Resident>> ListResidentsAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken) => await db.Residents.Where(x => x.CommunityId == communityId && x.UnitId == unitId).OrderByDescending(x => x.StartsOn).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<LeaseContract>> ListLeasesAsync(Guid communityId, CancellationToken cancellationToken) => await db.Leases.Where(x => x.CommunityId == communityId).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<AccessGrant>> ListActiveGrantsAsync(Guid communityId, CancellationToken cancellationToken) => await db.AccessGrants.Where(x => x.CommunityId == communityId && x.Status == AccessGrantStatus.Active).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<FinancialResponsibility>> ListResponsibilitiesAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken) => await db.FinancialResponsibilities.Where(x => x.CommunityId == communityId && x.UnitId == unitId).OrderByDescending(x => x.StartsOn).ToArrayAsync(cancellationToken);
    public Task<bool> IdentificationExistsAsync(Guid communityId, string identification, CancellationToken cancellationToken) => db.People.AnyAsync(x => x.CommunityId == communityId && x.Identification == identification, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}

internal static class MapBase
{
    public static void Apply<T>(EntityTypeBuilder<T> b, string table) where T : AppCondominio.SharedKernel.AggregateRoot
    { b.ToTable(table, "people"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever(); b.Ignore(x => x.DomainEvents); }
}
internal sealed class PersonConfig : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b) { MapBase.Apply(b,"People"); b.Property(x=>x.Identification).HasMaxLength(40).IsRequired(); b.Property(x=>x.DisplayName).HasMaxLength(250).IsRequired(); b.Property(x=>x.Email).HasMaxLength(320); b.Property(x=>x.Phone).HasMaxLength(60); b.Property(x=>x.Address).HasMaxLength(500); b.Property(x=>x.LegalRepresentative).HasMaxLength(250); b.HasIndex(x=>new{x.CommunityId,x.Identification}).IsUnique(); }
}
internal sealed class OwnershipConfig : IEntityTypeConfiguration<Ownership>
{
    public void Configure(EntityTypeBuilder<Ownership> b) { MapBase.Apply(b,"Ownerships"); b.Property(x=>x.Percentage).HasPrecision(18,6); b.Property(x=>x.AcquisitionType).HasMaxLength(80).IsRequired(); b.Property(x=>x.SupportDocumentReference).HasMaxLength(1000); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.PersonId,x.StartsOn}); }
}
internal sealed class LeaseConfig : IEntityTypeConfiguration<LeaseContract>
{
    public void Configure(EntityTypeBuilder<LeaseContract> b) { MapBase.Apply(b,"Leases"); b.Property(x=>x.PermissionsCsv).HasMaxLength(2000).IsRequired(); b.Property(x=>x.ContractDocumentReference).HasMaxLength(1000); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.EndsOn}); }
}
internal sealed class ResidentConfig : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> b) { MapBase.Apply(b,"Residents"); b.Ignore(x=>x.IsActive); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.PersonId,x.StartsOn}); }
}
internal sealed class AccessCodeConfig : IEntityTypeConfiguration<UnitAccessCode>
{
    public void Configure(EntityTypeBuilder<UnitAccessCode> b) { MapBase.Apply(b,"AccessCodes"); b.Property(x=>x.CodeHash).HasMaxLength(64).IsRequired(); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.ExpiresAtUtc}); }
}
internal sealed class ActivationConfig : IEntityTypeConfiguration<ActivationRequest>
{
    public void Configure(EntityTypeBuilder<ActivationRequest> b) { MapBase.Apply(b,"Activations"); b.Property(x=>x.ExternalUserId).HasMaxLength(200).IsRequired(); b.Property(x=>x.DecidedBy).HasMaxLength(200); b.Property(x=>x.DecisionReason).HasMaxLength(500); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.Status}); }
}
internal sealed class AccessGrantConfig : IEntityTypeConfiguration<AccessGrant>
{
    public void Configure(EntityTypeBuilder<AccessGrant> b) { MapBase.Apply(b,"AccessGrants"); b.Property(x=>x.ExternalUserId).HasMaxLength(200).IsRequired(); b.Property(x=>x.RevocationReason).HasMaxLength(500); b.HasIndex(x=>new{x.CommunityId,x.ExternalUserId,x.Status}); }
}
internal sealed class ResponsibilityConfig : IEntityTypeConfiguration<FinancialResponsibility>
{
    public void Configure(EntityTypeBuilder<FinancialResponsibility> b) { MapBase.Apply(b,"FinancialResponsibilities"); b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.StartsOn}); }
}

public static class DependencyInjection
{
    public static IServiceCollection AddPeopleModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("People") ?? configuration.GetConnectionString("Properties") ?? configuration.GetConnectionString("Organizations") ?? throw new InvalidOperationException("People connection string is required.");
        services.AddDbContext<PeopleDbContext>(o=>o.UseSqlServer(connectionString,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","people")));
        services.AddScoped<IPeopleRepository,EfPeopleRepository>(); services.AddScoped<PeopleService>(); return services;
    }
}
