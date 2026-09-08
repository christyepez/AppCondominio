using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Billing.Infrastructure;

internal sealed class EfMonthlyBillingRepository(BillingDbContext db):IMonthlyBillingRepository
{
    public Task<BillingPeriod?> GetPeriodAsync(Guid id,CancellationToken ct)=>db.Set<BillingPeriod>().FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<bool> PeriodExistsAsync(Guid communityId,int year,int month,CancellationToken ct)=>db.Set<BillingPeriod>().AnyAsync(x=>x.CommunityId==communityId&&x.Year==year&&x.Month==month,ct);
    public Task<ChargeConceptVersion?> GetVersionAsync(Guid id,CancellationToken ct)=>db.Versions.FirstOrDefaultAsync(x=>x.Id==id,ct);
    public Task<DiscountRule?> GetDiscountAsync(Guid communityId,Guid conceptId,DateOnly on,CancellationToken ct)=>db.DiscountRules.Where(x=>x.CommunityId==communityId&&x.ConceptId==conceptId&&x.ValidFrom<=on&&(x.ValidTo==null||x.ValidTo>=on)).OrderByDescending(x=>x.ValidFrom).FirstOrDefaultAsync(ct);
    public Task<InterestRule?> GetInterestAsync(Guid communityId,Guid conceptId,CancellationToken ct)=>db.InterestRules.Where(x=>x.CommunityId==communityId&&x.ConceptId==conceptId).OrderByDescending(x=>x.Id).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<DraftCharge>> GetDraftsAsync(Guid periodId,CancellationToken ct)=>await db.Set<DraftCharge>().Where(x=>x.PeriodId==periodId).OrderBy(x=>x.UnitId).ToArrayAsync(ct);
    public async Task<IReadOnlyList<BillingGenerationIssue>> GetIssuesAsync(Guid periodId,CancellationToken ct)=>await db.Set<BillingGenerationIssue>().Where(x=>x.PeriodId==periodId).ToArrayAsync(ct);
    public async Task<IReadOnlyList<ChargeObligation>> GetObligationsAsync(Guid periodId,CancellationToken ct)=>await db.Set<ChargeObligation>().Where(x=>x.PeriodId==periodId).ToArrayAsync(ct);
    public async Task<IReadOnlyList<ChargeObligation>> GetUnitObligationsAsync(Guid communityId,Guid unitId,CancellationToken ct)=>await db.Set<ChargeObligation>().Where(x=>x.CommunityId==communityId&&x.UnitId==unitId).ToArrayAsync(ct);
    public async Task DeleteDraftsAndIssuesAsync(Guid periodId,CancellationToken ct){await db.Set<DraftCharge>().Where(x=>x.PeriodId==periodId).ExecuteDeleteAsync(ct);await db.Set<BillingGenerationIssue>().Where(x=>x.PeriodId==periodId).ExecuteDeleteAsync(ct);}
    public async Task AddAsync<T>(T entity,CancellationToken ct) where T:class=>await db.Set<T>().AddAsync(entity,ct);
    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
}

internal sealed class BillingPeriodConfig:IEntityTypeConfiguration<BillingPeriod>{public void Configure(EntityTypeBuilder<BillingPeriod>b){BillingMap.Apply(b,"BillingPeriods");b.Property(x=>x.DraftTotal).HasPrecision(18,2);b.Property(x=>x.ApprovedBy).HasMaxLength(200);b.Property(x=>x.ReversalReason).HasMaxLength(500);b.HasIndex(x=>new{x.CommunityId,x.Year,x.Month}).IsUnique();}}
internal sealed class DraftChargeConfig:IEntityTypeConfiguration<DraftCharge>{public void Configure(EntityTypeBuilder<DraftCharge>b){BillingMap.Apply(b,"DraftCharges");b.Property(x=>x.Amount).HasPrecision(18,2);b.Property(x=>x.CalculationTrace).HasMaxLength(1000).IsRequired();b.HasIndex(x=>new{x.PeriodId,x.UnitId,x.ConceptId}).IsUnique();}}
internal sealed class GenerationIssueConfig:IEntityTypeConfiguration<BillingGenerationIssue>{public void Configure(EntityTypeBuilder<BillingGenerationIssue>b){BillingMap.Apply(b,"GenerationIssues");b.Property(x=>x.Code).HasMaxLength(80).IsRequired();b.Property(x=>x.Message).HasMaxLength(500).IsRequired();b.HasIndex(x=>x.PeriodId);}}
internal sealed class ChargeObligationConfig:IEntityTypeConfiguration<ChargeObligation>{public void Configure(EntityTypeBuilder<ChargeObligation>b){BillingMap.Apply(b,"ChargeObligations");b.Property(x=>x.OriginalAmount).HasPrecision(18,2);b.Property(x=>x.OutstandingAmount).HasPrecision(18,2);b.HasIndex(x=>x.DraftChargeId).IsUnique();b.HasIndex(x=>new{x.CommunityId,x.UnitId,x.Status});}}

public static class MonthlyBillingDependencyInjection
{
    public static IServiceCollection AddMonthlyBilling(this IServiceCollection services){services.AddScoped<IMonthlyBillingRepository,EfMonthlyBillingRepository>();services.AddScoped<MonthlyBillingService>();return services;}
}
