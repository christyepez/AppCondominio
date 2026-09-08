using AppCondominio.Modules.Properties.Application;
using AppCondominio.Modules.Properties.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Properties.Infrastructure;

public sealed class PropertiesDbContext(DbContextOptions<PropertiesDbContext> options) : DbContext(options)
{
    public DbSet<PropertyUnitType> UnitTypes => Set<PropertyUnitType>();
    public DbSet<PropertyUnit> Units => Set<PropertyUnit>();
    public DbSet<PropertyRelation> Relations => Set<PropertyRelation>();
    public DbSet<PropertyArea> Areas => Set<PropertyArea>();
    public DbSet<AliquotVersion> Aliquots => Set<AliquotVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PropertiesDbContext).Assembly);
}

internal sealed class EfPropertyRepository(PropertiesDbContext db) : IPropertyRepository
{
    public async Task AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class => await db.Set<T>().AddAsync(entity, cancellationToken);
    public Task<PropertyUnitType?> GetTypeAsync(Guid id, CancellationToken cancellationToken) => db.UnitTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<PropertyUnit?> GetUnitAsync(Guid id, CancellationToken cancellationToken) => db.Units.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyCollection<PropertyUnit>> ListUnitsAsync(Guid communityId, CancellationToken cancellationToken) => await db.Units.AsNoTracking().Where(x => x.CommunityId == communityId).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<PropertyArea>> ListAreasAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken) => await db.Areas.AsNoTracking().Where(x => x.CommunityId == communityId && x.UnitId == unitId).ToArrayAsync(cancellationToken);
    public async Task<IReadOnlyCollection<AliquotVersion>> ListAliquotsAsync(Guid communityId, CancellationToken cancellationToken) => await db.Aliquots.AsNoTracking().Where(x => x.CommunityId == communityId).ToArrayAsync(cancellationToken);
    public Task<bool> TypeCodeExistsAsync(Guid communityId, string code, CancellationToken cancellationToken) => db.UnitTypes.AnyAsync(x => x.CommunityId == communityId && x.Code == code, cancellationToken);
    public Task<bool> UnitCodeExistsAsync(Guid communityId, string code, CancellationToken cancellationToken) => db.Units.AnyAsync(x => x.CommunityId == communityId && x.Code == code, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}

internal static class PropertyMap
{
    public static void Aggregate<T>(EntityTypeBuilder<T> builder, string table) where T : AppCondominio.SharedKernel.AggregateRoot
    {
        builder.ToTable(table, "properties");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Ignore(x => x.DomainEvents);
    }
}

internal sealed class UnitTypeConfig : IEntityTypeConfiguration<PropertyUnitType>
{
    public void Configure(EntityTypeBuilder<PropertyUnitType> b)
    {
        PropertyMap.Aggregate(b, "UnitTypes");
        b.Property(x => x.Code).HasMaxLength(80).IsRequired();
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.HasIndex(x => new { x.CommunityId, x.Code }).IsUnique();
    }
}

internal sealed class UnitConfig : IEntityTypeConfiguration<PropertyUnit>
{
    public void Configure(EntityTypeBuilder<PropertyUnit> b)
    {
        PropertyMap.Aggregate(b, "Units");
        b.Property(x => x.Code).HasMaxLength(80).IsRequired();
        b.Property(x => x.Location).HasMaxLength(300).IsRequired();
        b.Property(x => x.MainAreaM2).HasPrecision(18, 4);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.HasIndex(x => new { x.CommunityId, x.Code }).IsUnique();
        b.HasIndex(x => x.TypeId);
    }
}

internal sealed class RelationConfig : IEntityTypeConfiguration<PropertyRelation>
{
    public void Configure(EntityTypeBuilder<PropertyRelation> b)
    {
        PropertyMap.Aggregate(b, "Relations");
        b.HasIndex(x => new { x.CommunityId, x.MainUnitId, x.RelatedUnitId, x.StartsOn });
    }
}

internal sealed class AreaConfig : IEntityTypeConfiguration<PropertyArea>
{
    public void Configure(EntityTypeBuilder<PropertyArea> b)
    {
        PropertyMap.Aggregate(b, "Areas");
        b.Property(x => x.AreaM2).HasPrecision(18, 4);
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => new { x.CommunityId, x.UnitId });
    }
}

internal sealed class AliquotConfig : IEntityTypeConfiguration<AliquotVersion>
{
    public void Configure(EntityTypeBuilder<AliquotVersion> b)
    {
        PropertyMap.Aggregate(b, "AliquotVersions");
        b.Property(x => x.Value).HasPrecision(18, 6);
        b.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        b.Property(x => x.SupportDocumentReference).HasMaxLength(1000);
        b.Property(x => x.ApprovedBy).HasMaxLength(200);
        b.HasIndex(x => new { x.CommunityId, x.UnitId, x.ValidFrom });
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddPropertiesModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Properties")
            ?? configuration.GetConnectionString("Communities")
            ?? configuration.GetConnectionString("Organizations")
            ?? throw new InvalidOperationException("Properties connection string is required.");

        services.AddDbContext<PropertiesDbContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "properties")));
        services.AddScoped<IPropertyRepository, EfPropertyRepository>();
        services.AddScoped<PropertyService>();
        return services;
    }
}
