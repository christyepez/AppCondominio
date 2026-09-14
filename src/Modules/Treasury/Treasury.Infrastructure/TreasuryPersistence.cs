using AppCondominio.Modules.Treasury.Application;
using AppCondominio.Modules.Treasury.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Treasury.Infrastructure;

public sealed class TreasuryDbContext(DbContextOptions<TreasuryDbContext> options):DbContext(options)
{
    public DbSet<SupplierPayable> Payables=>Set<SupplierPayable>();public DbSet<PaymentRequest> PaymentRequests=>Set<PaymentRequest>();public DbSet<TreasuryDisbursement> Disbursements=>Set<TreasuryDisbursement>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(TreasuryDbContext).Assembly);
}
internal sealed class EfTreasuryRepository(TreasuryDbContext db):ITreasuryRepository
{
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task<SupplierPayable?> GetPayableAsync(Guid id,CancellationToken ct)=>db.Payables.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<PaymentRequest?> GetRequestAsync(Guid id,CancellationToken ct)=>db.PaymentRequests.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<TreasuryDisbursement?> GetDisbursementAsync(Guid id,CancellationToken ct)=>db.Disbursements.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> DocumentExistsAsync(Guid communityId,Guid supplierId,string documentNumber,CancellationToken ct)=>db.Payables.AnyAsync(x=>x.CommunityId==communityId&&x.SupplierId==supplierId&&x.DocumentNumber==documentNumber,ct);
    public Task<bool> PaymentReferenceExistsAsync(Guid communityId,string paymentReference,CancellationToken ct)=>db.Disbursements.AnyAsync(x=>x.CommunityId==communityId&&x.PaymentReference==paymentReference,ct);
    public async Task<IReadOnlyList<SupplierPayable>> GetOutstandingAsync(Guid communityId,CancellationToken ct)=>await db.Payables.Where(x=>x.CommunityId==communityId&&x.OutstandingAmount>0).AsNoTracking().ToArrayAsync(ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}
internal static class TreasuryMap{public static void Apply<T>(EntityTypeBuilder<T>b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"treasury");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}}
internal sealed class PayableConfig:IEntityTypeConfiguration<SupplierPayable>{public void Configure(EntityTypeBuilder<SupplierPayable>b){TreasuryMap.Apply(b,"SupplierPayables");b.Property(x=>x.DocumentNumber).HasMaxLength(80).IsRequired();b.Property(x=>x.OriginalAmount).HasPrecision(18,2);b.Property(x=>x.OutstandingAmount).HasPrecision(18,2);b.Property(x=>x.ExpenseAccount).HasMaxLength(40).IsRequired();b.Property(x=>x.PayableAccount).HasMaxLength(40).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.SupplierId,x.DocumentNumber}).IsUnique();}}
internal sealed class RequestConfig:IEntityTypeConfiguration<PaymentRequest>{public void Configure(EntityTypeBuilder<PaymentRequest>b){TreasuryMap.Apply(b,"PaymentRequests");b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.RequestedBy).HasMaxLength(200).IsRequired();b.Property(x=>x.DecidedBy).HasMaxLength(200);b.Property(x=>x.DecisionReason).HasMaxLength(500);b.HasIndex(x=>x.PayableId);}}
internal sealed class DisbursementConfig:IEntityTypeConfiguration<TreasuryDisbursement>{public void Configure(EntityTypeBuilder<TreasuryDisbursement>b){TreasuryMap.Apply(b,"Disbursements");b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.PaymentReference).HasMaxLength(120).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.PaymentReference}).IsUnique();b.HasIndex(x=>x.PaymentRequestId).IsUnique();}}
public static class DependencyInjection{public static IServiceCollection AddTreasuryModule(this IServiceCollection services,IConfiguration configuration){var cs=configuration.GetConnectionString("Treasury")??configuration.GetConnectionString("Accounting")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Treasury connection string is required.");services.AddDbContext<TreasuryDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","treasury")));services.AddScoped<ITreasuryRepository,EfTreasuryRepository>();services.AddScoped<TreasuryService>();return services;}}
