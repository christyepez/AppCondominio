using AppCondominio.Contracts.Tax;
using AppCondominio.Modules.Tax.Application;
using AppCondominio.Modules.Tax.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Tax.Infrastructure;

public sealed class TaxDbContext(DbContextOptions<TaxDbContext> options):DbContext(options)
{
    public DbSet<ElectronicDocument> Documents=>Set<ElectronicDocument>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaxDbContext).Assembly);
}

internal sealed class EfTaxRepository(TaxDbContext db):ITaxRepository
{
    public Task<bool> SourceExistsAsync(Guid communityId,Guid sourceDocumentId,CancellationToken ct)=>db.Documents.AnyAsync(x=>x.CommunityId==communityId&&x.SourceDocumentId==sourceDocumentId,ct);
    public Task<ElectronicDocument?> GetAsync(Guid id,CancellationToken ct)=>db.Documents.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task AddAsync(ElectronicDocument document,CancellationToken ct)=>await db.Documents.AddAsync(document,ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal sealed class ElectronicDocumentConfig:IEntityTypeConfiguration<ElectronicDocument>
{
    public void Configure(EntityTypeBuilder<ElectronicDocument>b)
    {
        b.ToTable("ElectronicDocuments","tax");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);
        b.Property(x=>x.DocumentType).HasMaxLength(40).IsRequired();b.Property(x=>x.EstablishmentCode).HasMaxLength(10).IsRequired();b.Property(x=>x.EmissionPoint).HasMaxLength(10).IsRequired();b.Property(x=>x.Sequential).HasMaxLength(20).IsRequired();b.Property(x=>x.AccessKey).HasMaxLength(80).IsRequired();b.Property(x=>x.PayloadReference).HasMaxLength(500).IsRequired();b.Property(x=>x.AuthorizationNumber).HasMaxLength(100);b.Property(x=>x.LastMessage).HasMaxLength(1000);
        b.HasIndex(x=>new{x.CommunityId,x.SourceDocumentId}).IsUnique();b.HasIndex(x=>x.AccessKey).IsUnique();
    }
}

public sealed class PendingConfigurationElectronicInvoicingService:IElectronicInvoicingService
{
    public Task<ElectronicDocumentResult> SubmitAsync(ElectronicDocumentRequest request,CancellationToken cancellationToken=default)=>Task.FromResult(new ElectronicDocumentResult("pendingconfiguration",null,null,"SRI provider is not configured for this environment."));
    public Task<ElectronicDocumentResult> GetStatusAsync(Guid communityId,string accessKey,CancellationToken cancellationToken=default)=>Task.FromResult(new ElectronicDocumentResult("pendingconfiguration",null,null,"SRI provider is not configured for this environment."));
}

public static class DependencyInjection
{
    public static IServiceCollection AddTaxModule(this IServiceCollection services,IConfiguration configuration)
    {
        var cs=configuration.GetConnectionString("Tax")??configuration.GetConnectionString("Banking")??configuration.GetConnectionString("Collections")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Tax connection string is required.");
        services.AddDbContext<TaxDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","tax")));
        services.AddScoped<ITaxRepository,EfTaxRepository>();services.AddScoped<IElectronicInvoicingService,PendingConfigurationElectronicInvoicingService>();services.AddScoped<TaxService>();return services;
    }
}
