using AppCondominio.Modules.Budgeting.Application;
using AppCondominio.Modules.Budgeting.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Budgeting.Infrastructure;

public sealed class BudgetingDbContext(DbContextOptions<BudgetingDbContext> options):DbContext(options)
{
    public DbSet<BudgetPlan> Plans=>Set<BudgetPlan>();public DbSet<BudgetLine> Lines=>Set<BudgetLine>();public DbSet<BudgetActual> Actuals=>Set<BudgetActual>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)=>modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetingDbContext).Assembly);
}

internal sealed class EfBudgetingRepository(BudgetingDbContext db):IBudgetingRepository
{
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task<BudgetPlan?> GetPlanAsync(Guid id,CancellationToken ct)=>db.Plans.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<BudgetLine?> GetLineAsync(Guid id,CancellationToken ct)=>db.Lines.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> PlanVersionExistsAsync(Guid communityId,int year,int version,CancellationToken ct)=>db.Plans.AnyAsync(x=>x.CommunityId==communityId&&x.Year==year&&x.Version==version,ct);
    public Task<bool> LineExistsAsync(Guid budgetPlanId,string accountCode,int month,CancellationToken ct)=>db.Lines.AnyAsync(x=>x.BudgetPlanId==budgetPlanId&&x.AccountCode==accountCode&&x.Month==month,ct);
    public Task<bool> ActualSourceExistsAsync(Guid communityId,string sourceType,string sourceReference,CancellationToken ct)=>db.Actuals.AnyAsync(x=>x.CommunityId==communityId&&x.SourceType==sourceType&&x.SourceReference==sourceReference,ct);
    public async Task<IReadOnlyList<BudgetLine>> GetLinesAsync(Guid budgetPlanId,CancellationToken ct)=>await db.Lines.Where(x=>x.BudgetPlanId==budgetPlanId).AsNoTracking().ToArrayAsync(ct);
    public async Task<IReadOnlyList<BudgetActual>> GetActualsAsync(Guid communityId,int year,CancellationToken ct)=>await db.Actuals.Where(x=>x.CommunityId==communityId&&x.Year==year).AsNoTracking().ToArrayAsync(ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal static class BudgetingMap{public static void Apply<T>(EntityTypeBuilder<T>b,string table) where T:AppCondominio.SharedKernel.AggregateRoot{b.ToTable(table,"budgeting");b.HasKey(x=>x.Id);b.Property(x=>x.Id).ValueGeneratedNever();b.Ignore(x=>x.DomainEvents);}}
internal sealed class PlanConfig:IEntityTypeConfiguration<BudgetPlan>{public void Configure(EntityTypeBuilder<BudgetPlan>b){BudgetingMap.Apply(b,"BudgetPlans");b.Property(x=>x.Name).HasMaxLength(200).IsRequired();b.Property(x=>x.ApprovedBy).HasMaxLength(200);b.Property(x=>x.ClosedBy).HasMaxLength(200);b.HasIndex(x=>new{x.CommunityId,x.Year,x.Version}).IsUnique();}}
internal sealed class LineConfig:IEntityTypeConfiguration<BudgetLine>{public void Configure(EntityTypeBuilder<BudgetLine>b){BudgetingMap.Apply(b,"BudgetLines");b.Property(x=>x.AccountCode).HasMaxLength(40).IsRequired();b.Property(x=>x.Category).HasMaxLength(120).IsRequired();b.Property(x=>x.PlannedAmount).HasPrecision(18,2);b.HasIndex(x=>new{x.BudgetPlanId,x.AccountCode,x.Month}).IsUnique();}}
internal sealed class ActualConfig:IEntityTypeConfiguration<BudgetActual>{public void Configure(EntityTypeBuilder<BudgetActual>b){BudgetingMap.Apply(b,"BudgetActuals");b.Property(x=>x.AccountCode).HasMaxLength(40).IsRequired();b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.SourceType).HasMaxLength(80).IsRequired();b.Property(x=>x.SourceReference).HasMaxLength(200).IsRequired();b.HasIndex(x=>new{x.CommunityId,x.SourceType,x.SourceReference}).IsUnique();}}

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingModule(this IServiceCollection services,IConfiguration configuration)
    {
        var cs=configuration.GetConnectionString("Budgeting")??configuration.GetConnectionString("Treasury")??configuration.GetConnectionString("Accounting")??configuration.GetConnectionString("Organizations")??throw new InvalidOperationException("Budgeting connection string is required.");
        services.AddDbContext<BudgetingDbContext>(o=>o.UseSqlServer(cs,sql=>sql.MigrationsHistoryTable("__EFMigrationsHistory","budgeting")));services.AddScoped<IBudgetingRepository,EfBudgetingRepository>();services.AddScoped<BudgetingService>();return services;
    }
}
