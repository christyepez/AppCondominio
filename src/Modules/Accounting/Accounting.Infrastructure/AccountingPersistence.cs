using AppCondominio.Modules.Accounting.Application;
using AppCondominio.Modules.Accounting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Accounting.Infrastructure;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options):DbContext(options)
{
    public DbSet<LedgerAccount> Accounts=>Set<LedgerAccount>();public DbSet<AccountingPeriod> Periods=>Set<AccountingPeriod>();public DbSet<JournalEntry> Journals=>Set<JournalEntry>();public DbSet<JournalLine> Lines=>Set<JournalLine>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
}

internal sealed class EfAccountingRepository(AccountingDbContext db):IAccountingRepository
{
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task<LedgerAccount?> GetAccountAsync(Guid id,CancellationToken ct)=>db.Accounts.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<AccountingPeriod?> GetPeriodAsync(Guid id,CancellationToken ct)=>db.Periods.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> PeriodExistsAsync(Guid communityId,int year,int month,CancellationToken ct)=>db.Periods.AnyAsync(x=>x.CommunityId==communityId&&x.Year==year&&x.Month==month,ct);
    public Task<bool> SourceExistsAsync(Guid communityId,string sourceType,string sourceReference,CancellationToken ct)=>db.Journals.AnyAsync(x=>x.CommunityId==communityId&&x.SourceType==sourceType&&x.SourceReference==sourceReference,ct);
    public Task<JournalEntry?> GetJournalAsync(Guid id,CancellationToken ct)=>db.Journals.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<JournalLine>> GetJournalLinesAsync(Guid journalId,CancellationToken ct)=>await db.Lines.Where(x=>x.JournalEntryId==journalId).ToArrayAsync(ct);
    public async Task<IReadOnlyList<(JournalEntry Journal,JournalLine Line,LedgerAccount Account)>> GetPostedLinesAsync(Guid communityId,CancellationToken ct)
    {
        var query=from line in db.Lines join journal in db.Journals on line.JournalEntryId equals journal.Id join account in db.Accounts on line.AccountId equals account.Id where journal.CommunityId==communityId&&journal.Status==JournalStatus.Posted select new{journal,line,account};
        var rows=await query.AsNoTracking().ToArrayAsync(ct);return rows.Select(x=>(x.journal,x.line,x.account)).ToArray();
    }
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class AccountingMap{public static void Apply<T>(EntityTypeBuilder<T>b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"accounting");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}}
internal sealed class AccountConfig:IEntityTypeConfiguration<LedgerAccount>{public void Configure(EntityTypeBuilder<LedgerAccount>b){AccountingMap.Apply(b,"LedgerAccounts");b.Property(x=>x.Code).HasMaxLength(40).IsRequired();b.Property(x=>x.Name).HasMaxLength(200).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.Code}).IsUnique();}}
internal sealed class PeriodConfig:IEntityTypeConfiguration<AccountingPeriod>{public void Configure(EntityTypeBuilder<AccountingPeriod>b){AccountingMap.Apply(b,"AccountingPeriods");b.Property(x=>x.ClosedBy).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.Year,x.Month}).IsUnique();}}
internal sealed class JournalConfig:IEntityTypeConfiguration<JournalEntry>{public void Configure(EntityTypeBuilder<JournalEntry>b){AccountingMap.Apply(b,"JournalEntries");b.Property(x=>x.Description).HasMaxLength(500).IsRequired();b.Property(x=>x.SourceType).HasMaxLength(80).IsRequired();b.Property(x=>x.SourceReference).HasMaxLength(200).IsRequired();b.Property(x=>x.PostedBy).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.SourceType,x.SourceReference}).IsUnique();}}
internal sealed class LineConfig:IEntityTypeConfiguration<JournalLine>{public void Configure(EntityTypeBuilder<JournalLine>b){AccountingMap.Apply(b,"JournalLines");b.Property(x=>x.AccountCode).HasMaxLength(40).IsRequired();b.Property(x=>x.Debit).HasPrecision(18,2);b.Property(x=>x.Credit).HasPrecision(18,2);b.Property(x=>x.CostCenter).HasMaxLength(80);b.HasIndex(x=>x.JournalEntryId);}}

public static class DependencyInjection
{
    public static IServiceCollection AddAccountingModule(this IServiceCollection services,IConfiguration configuration)
    {
        var cs=configuration.GetConnectionString("Accounting")??configuration.GetConnectionString("Tax")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Accounting connection string is required.");
        services.AddDbContext<AccountingDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","accounting")));services.AddScoped<IAccountingRepository,EfAccountingRepository>();services.AddScoped<AccountingService>();return services;
    }
}
