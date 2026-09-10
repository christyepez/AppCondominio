using AppCondominio.Modules.Banking.Application;
using AppCondominio.Modules.Banking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Banking.Infrastructure;

public sealed class BankingDbContext(DbContextOptions<BankingDbContext> options):DbContext(options)
{
    public DbSet<BankAccount> Accounts=>Set<BankAccount>();
    public DbSet<BankTransaction> Transactions=>Set<BankTransaction>();
    public DbSet<Reconciliation> Reconciliations=>Set<Reconciliation>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingDbContext).Assembly);
}

internal sealed class EfBankingRepository(BankingDbContext db):IBankingRepository
{
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task<bool> AccountExistsAsync(Guid communityId,string bankCode,string accountNumber,CancellationToken ct)=>db.Accounts.AnyAsync(x=>x.CommunityId==communityId&&x.BankCode==bankCode&&x.AccountNumber==accountNumber,ct);
    public Task<bool> ImportKeyExistsAsync(Guid bankAccountId,string importKey,CancellationToken ct)=>db.Transactions.AnyAsync(x=>x.BankAccountId==bankAccountId&&x.ImportKey==importKey,ct);
    public Task<BankTransaction?> GetTransactionAsync(Guid id,CancellationToken ct)=>db.Transactions.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<Reconciliation?> GetReconciliationByTransactionAsync(Guid transactionId,CancellationToken ct)=>db.Reconciliations.FirstOrDefaultAsync(x=>x.BankTransactionId==transactionId,ct);
    public async Task<IReadOnlyList<BankTransaction>> ListUnmatchedCreditsAsync(Guid communityId,CancellationToken ct)=>await db.Transactions.Where(x=>x.CommunityId==communityId&&x.Type==BankTransactionType.Credit&&!db.Reconciliations.Any(r=>r.BankTransactionId==x.Id&&r.Status==ReconciliationStatus.Matched)).OrderBy(x=>x.BookingDate).ToArrayAsync(ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class BankingMap
{
    public static void Apply<T>(EntityTypeBuilder<T>b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"banking");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}
}
internal sealed class BankAccountConfig:IEntityTypeConfiguration<BankAccount>{public void Configure(EntityTypeBuilder<BankAccount>b){BankingMap.Apply(b,"BankAccounts");b.Property(x=>x.BankCode).HasMaxLength(40).IsRequired();b.Property(x=>x.AccountNumber).HasMaxLength(80).IsRequired();b.Property(x=>x.Currency).HasMaxLength(3).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.BankCode,x.AccountNumber}).IsUnique();}}
internal sealed class BankTransactionConfig:IEntityTypeConfiguration<BankTransaction>{public void Configure(EntityTypeBuilder<BankTransaction>b){BankingMap.Apply(b,"BankTransactions");b.Property(x=>x.ImportKey).HasMaxLength(160).IsRequired();b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.Reference).HasMaxLength(300).IsRequired();b.Property(x=>x.Description).HasMaxLength(1000).IsRequired();b.HasIndex(x=>new{x.BankAccountId,x.ImportKey}).IsUnique();b.HasIndex(x=>new{x.CommunityId,x.BookingDate,x.Type});}}
internal sealed class ReconciliationConfig:IEntityTypeConfiguration<Reconciliation>{public void Configure(EntityTypeBuilder<Reconciliation>b){BankingMap.Apply(b,"Reconciliations");b.Property(x=>x.Confidence).HasPrecision(5,4);b.Property(x=>x.Reason).HasMaxLength(500);b.Property(x=>x.DecidedBy).HasMaxLength(200);b.HasIndex(x=>x.BankTransactionId).IsUnique();b.HasIndex(x=>new{x.CommunityId,x.Status});}}

public static class DependencyInjection
{
    public static IServiceCollection AddBankingModule(this IServiceCollection services,IConfiguration configuration)
    {
        string cs=configuration.GetConnectionString("Banking")??configuration.GetConnectionString("Collections")??configuration.GetConnectionString("Billing")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Banking connection string is required.");
        services.AddDbContext<BankingDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","banking")));
        services.AddScoped<IBankingRepository,EfBankingRepository>();services.AddScoped<BankingService>();return services;
    }
}
