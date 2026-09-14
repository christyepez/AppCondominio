using AppCondominio.Modules.Collections.Application;
using AppCondominio.Modules.Collections.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Collections.Infrastructure;

public sealed class CollectionsDbContext(DbContextOptions<CollectionsDbContext> options):DbContext(options)
{
    public DbSet<Receivable> Receivables=>Set<Receivable>();
    public DbSet<Payment> Payments=>Set<Payment>();
    public DbSet<PaymentApplication> Applications=>Set<PaymentApplication>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollectionsDbContext).Assembly);
}

internal sealed class EfCollectionsRepository(CollectionsDbContext db):ICollectionsRepository
{
    public Task<bool> ReceivableExistsForBillingObligationAsync(Guid billingObligationId,CancellationToken ct)=>db.Receivables.AnyAsync(x=>x.BillingObligationId==billingObligationId,ct);
    public Task<bool> PaymentReferenceExistsAsync(Guid communityId,string reference,string? externalTransactionId,CancellationToken ct)=>db.Payments.AnyAsync(x=>x.CommunityId==communityId&&(x.Reference==reference||(externalTransactionId!=null&&x.ExternalTransactionId==externalTransactionId)),ct);
    public Task<Receivable?> GetReceivableAsync(Guid id,CancellationToken ct)=>db.Receivables.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<Payment?> GetPaymentAsync(Guid id,CancellationToken ct)=>db.Payments.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<PaymentApplication>> GetApplicationsForPaymentAsync(Guid paymentId,CancellationToken ct)=>await db.Applications.Where(x=>x.PaymentId==paymentId).OrderBy(x=>x.AppliedAtUtc).ToArrayAsync(ct);
    public async Task<IReadOnlyList<Receivable>> GetOpenReceivablesAsync(Guid communityId,Guid? unitId,CancellationToken ct)=>await db.Receivables.Where(x=>x.CommunityId==communityId&&(unitId==null||x.UnitId==unitId)&&(x.Status==ReceivableStatus.Open||x.Status==ReceivableStatus.PartiallyPaid)).OrderBy(x=>x.DueOn).ToArrayAsync(ct);
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class CollectionsMap
{
    public static void Apply<T>(EntityTypeBuilder<T> b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"collections");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}
}
internal sealed class ReceivableConfig:IEntityTypeConfiguration<Receivable>{public void Configure(EntityTypeBuilder<Receivable>b){CollectionsMap.Apply(b,"Receivables");b.Property(x=>x.OriginalAmount).HasPrecision(18,2);b.Property(x=>x.OutstandingAmount).HasPrecision(18,2);b.HasIndex(x=>x.BillingObligationId).IsUnique();b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.Status,x.DueOn});}}
internal sealed class PaymentConfig:IEntityTypeConfiguration<Payment>{public void Configure(EntityTypeBuilder<Payment>b){CollectionsMap.Apply(b,"Payments");b.Property(x=>x.Reference).HasMaxLength(120).IsRequired();b.Property(x=>x.ExternalTransactionId).HasMaxLength(200);b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.UnappliedAmount).HasPrecision(18,2);b.HasIndex(x=>new{x.CommunityId,x.Reference}).IsUnique();b.HasIndex(x=>new{x.CommunityId,x.ExternalTransactionId});}}
internal sealed class PaymentApplicationConfig:IEntityTypeConfiguration<PaymentApplication>{public void Configure(EntityTypeBuilder<PaymentApplication>b){CollectionsMap.Apply(b,"PaymentApplications");b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.AppliedBy).HasMaxLength(200).IsRequired();b.Property(x=>x.ReversedBy).HasMaxLength(200);b.HasIndex(x=>new{x.PaymentId,x.ReceivableId});}}

public static class DependencyInjection
{
    public static IServiceCollection AddCollectionsModule(this IServiceCollection services,IConfiguration configuration)
    {
        string cs=configuration.GetConnectionString("Collections")??configuration.GetConnectionString("Billing")??configuration.GetConnectionString("People")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Collections connection string is required.");
        services.AddDbContext<CollectionsDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","collections")));
        services.AddScoped<ICollectionsRepository,EfCollectionsRepository>();services.AddScoped<CollectionsService>();return services;
    }
}
