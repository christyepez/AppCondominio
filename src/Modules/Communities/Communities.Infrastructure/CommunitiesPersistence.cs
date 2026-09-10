using AppCondominio.Modules.Communities.Application;
using AppCondominio.Modules.Communities.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Communities.Infrastructure;

public sealed class CommunitiesDbContext(DbContextOptions<CommunitiesDbContext> options):DbContext(options)
{
    public DbSet<Community> Communities=>Set<Community>(); public DbSet<TaxSettings> TaxSettings=>Set<TaxSettings>();
    public DbSet<StructureNode> StructureNodes=>Set<StructureNode>(); public DbSet<CommonArea> CommonAreas=>Set<CommonArea>();
    public DbSet<CommunityBankAccount> BankAccounts=>Set<CommunityBankAccount>(); public DbSet<ChargeSettings> ChargeSettings=>Set<ChargeSettings>();
    public DbSet<CommunityAuthority> Authorities=>Set<CommunityAuthority>(); public DbSet<CommunityDocument> Documents=>Set<CommunityDocument>();
    protected override void OnModelCreating(ModelBuilder b)=>b.ApplyConfigurationsFromAssembly(typeof(CommunitiesDbContext).Assembly);
}

internal sealed class EfCommunityRepository(CommunitiesDbContext db):ICommunityRepository
{
    public Task<Community?> GetCommunityAsync(Guid id,CancellationToken ct)=>db.Communities.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public async Task<IReadOnlyCollection<Community>> ListAsync(CancellationToken ct)=>await db.Communities.AsNoTracking().ToArrayAsync(ct);
    public Task<int> CountAsync<T>(Guid communityId,CancellationToken ct) where T:class=>db.Set<T>().CountAsync(x=>EF.Property<Guid>(x,"CommunityId")==communityId,ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class Map
{
    public static void Aggregate<T>(EntityTypeBuilder<T> b,string table) where T:AppCondominio.SharedKernel.AggregateRoot {b.ToTable(table,"communities");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}
}
internal sealed class CommunityConfig:IEntityTypeConfiguration<Community>{public void Configure(EntityTypeBuilder<Community>b){Map.Aggregate(b,"Communities");b.Property(x=>x.Code).HasMaxLength(80).IsRequired();b.Property(x=>x.Name).HasMaxLength(200).IsRequired();b.Property(x=>x.TaxId).HasMaxLength(20);b.Property(x=>x.Address).HasMaxLength(500).IsRequired();b.Property(x=>x.CommunityType).HasMaxLength(80);b.Property(x=>x.AdministratorName).HasMaxLength(200);b.Property(x=>x.PresidentName).HasMaxLength(200);b.HasIndex(x=>new{x.OrganizationId,x.Code}).IsUnique();}}
internal sealed class TaxConfig:IEntityTypeConfiguration<TaxSettings>{public void Configure(EntityTypeBuilder<TaxSettings>b){Map.Aggregate(b,"TaxSettings");b.HasIndex(x=>x.CommunityId).IsUnique();b.Property(x=>x.LegalName).HasMaxLength(250);b.Property(x=>x.TaxId).HasMaxLength(20);b.Property(x=>x.Establishment).HasMaxLength(10);b.Property(x=>x.EmissionPoint).HasMaxLength(10);b.Property(x=>x.SriEnvironment).HasMaxLength(20);}}
internal sealed class NodeConfig:IEntityTypeConfiguration<StructureNode>{public void Configure(EntityTypeBuilder<StructureNode>b){Map.Aggregate(b,"StructureNodes");b.Property(x=>x.Code).HasMaxLength(80);b.Property(x=>x.Name).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.Code}).IsUnique();b.HasIndex(x=>x.ParentId);}}
internal sealed class AreaConfig:IEntityTypeConfiguration<CommonArea>{public void Configure(EntityTypeBuilder<CommonArea>b){Map.Aggregate(b,"CommonAreas");b.Property(x=>x.Code).HasMaxLength(80);b.Property(x=>x.Name).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.Code}).IsUnique();}}
internal sealed class BankConfig:IEntityTypeConfiguration<CommunityBankAccount>{public void Configure(EntityTypeBuilder<CommunityBankAccount>b){Map.Aggregate(b,"BankAccounts");b.Property(x=>x.Bank).HasMaxLength(160);b.Property(x=>x.Number).HasMaxLength(80);b.Property(x=>x.Currency).HasMaxLength(3);b.Property(x=>x.Use).HasMaxLength(160);b.Property(x=>x.AccountingAccount).HasMaxLength(80);}}
internal sealed class ChargeConfig:IEntityTypeConfiguration<ChargeSettings>{public void Configure(EntityTypeBuilder<ChargeSettings>b){Map.Aggregate(b,"ChargeSettings");b.HasIndex(x=>x.CommunityId).IsUnique();b.Property(x=>x.InterestRate).HasPrecision(18,6);b.Property(x=>x.ApplicationOrder).HasMaxLength(300);b.Property(x=>x.Currency).HasMaxLength(3);}}
internal sealed class AuthorityConfig:IEntityTypeConfiguration<CommunityAuthority>{public void Configure(EntityTypeBuilder<CommunityAuthority>b){Map.Aggregate(b,"Authorities");b.Property(x=>x.Role).HasMaxLength(80);b.Property(x=>x.PersonName).HasMaxLength(200);b.Property(x=>x.SupportDocumentReference).HasMaxLength(1000);b.HasIndex(x=>new{x.CommunityId,x.Role,x.StartsOn});}}
internal sealed class DocumentConfig:IEntityTypeConfiguration<CommunityDocument>{public void Configure(EntityTypeBuilder<CommunityDocument>b){Map.Aggregate(b,"Documents");b.Property(x=>x.DocumentType).HasMaxLength(80);b.Property(x=>x.Name).HasMaxLength(250);b.Property(x=>x.ExternalReference).HasMaxLength(1000);b.HasIndex(x=>x.CommunityId);}}

public static class DependencyInjection
{
    public static IServiceCollection AddCommunitiesModule(this IServiceCollection services,IConfiguration configuration)
    {
        var cs=configuration.GetConnectionString("Communities")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Communities connection string is required.");
        services.AddDbContext<CommunitiesDbContext>(o=>o.UseSqlServer(cs,s=>s.MigrationsHistoryTable("__EFMigrationsHistory","communities")));
        services.AddScoped<ICommunityRepository,EfCommunityRepository>();services.AddScoped<CommunityService>();return services;
    }
}
