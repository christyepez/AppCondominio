using AppCondominio.Modules.Budgeting.Domain;

namespace AppCondominio.Modules.Budgeting.Application;

public interface IBudgetingRepository
{
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<BudgetPlan?> GetPlanAsync(Guid id,CancellationToken ct);
    Task<BudgetLine?> GetLineAsync(Guid id,CancellationToken ct);
    Task<bool> PlanVersionExistsAsync(Guid communityId,int year,int version,CancellationToken ct);
    Task<bool> LineExistsAsync(Guid budgetPlanId,string accountCode,int month,CancellationToken ct);
    Task<bool> ActualSourceExistsAsync(Guid communityId,string sourceType,string sourceReference,CancellationToken ct);
    Task<IReadOnlyList<BudgetPlan>> GetPlansAsync(Guid communityId,CancellationToken ct);
    Task<IReadOnlyList<BudgetLine>> GetLinesAsync(Guid budgetPlanId,CancellationToken ct);
    Task<IReadOnlyList<BudgetActual>> GetActualsAsync(Guid communityId,int year,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record BudgetPlanOption(Guid Id,int Year,int Version,string Name,BudgetStatus Status);

public sealed class BudgetingService(IBudgetingRepository repository)
{
    public async Task<IReadOnlyList<BudgetPlanOption>> ListPlansAsync(Guid communityId,CancellationToken ct)=>(await repository.GetPlansAsync(communityId,ct)).OrderByDescending(x=>x.Year).ThenByDescending(x=>x.Version).Select(x=>new BudgetPlanOption(x.Id,x.Year,x.Version,x.Name,x.Status)).ToArray();

    public async Task<Guid> CreatePlanAsync(Guid communityId,int year,int version,string name,CancellationToken ct)
    {
        if(await repository.PlanVersionExistsAsync(communityId,year,version,ct))throw new InvalidOperationException("Budget version already exists.");
        var plan=BudgetPlan.Create(communityId,year,version,name);await repository.AddAsync(plan,ct);await repository.SaveChangesAsync(ct);return plan.Id;
    }

    public async Task<Guid> AddLineAsync(Guid planId,string accountCode,string category,int month,decimal plannedAmount,CancellationToken ct)
    {
        var plan=await repository.GetPlanAsync(planId,ct)??throw new KeyNotFoundException("Budget plan not found.");
        if(plan.Status!=BudgetStatus.Draft)throw new InvalidOperationException("Only draft budgets can be edited.");
        if(await repository.LineExistsAsync(planId,accountCode,month,ct))throw new InvalidOperationException("Budget line already exists for account and month.");
        var line=BudgetLine.Create(plan.Id,plan.CommunityId,accountCode,category,month,plannedAmount);await repository.AddAsync(line,ct);await repository.SaveChangesAsync(ct);return line.Id;
    }

    public async Task ChangeLineAmountAsync(Guid lineId,decimal amount,CancellationToken ct)
    {
        var line=await repository.GetLineAsync(lineId,ct)??throw new KeyNotFoundException("Budget line not found.");var plan=await repository.GetPlanAsync(line.BudgetPlanId,ct)??throw new KeyNotFoundException("Budget plan not found.");
        if(plan.Status!=BudgetStatus.Draft)throw new InvalidOperationException("Only draft budgets can be edited.");line.ChangeAmount(amount);await repository.SaveChangesAsync(ct);
    }

    public async Task ApproveAsync(Guid planId,string by,CancellationToken ct)
    {
        var plan=await repository.GetPlanAsync(planId,ct)??throw new KeyNotFoundException("Budget plan not found.");var lines=await repository.GetLinesAsync(planId,ct);if(lines.Count==0)throw new InvalidOperationException("Budget requires at least one line.");plan.Approve(by);await repository.SaveChangesAsync(ct);
    }

    public async Task CloseAsync(Guid planId,string by,CancellationToken ct){var plan=await repository.GetPlanAsync(planId,ct)??throw new KeyNotFoundException("Budget plan not found.");plan.Close(by);await repository.SaveChangesAsync(ct);}

    public async Task<Guid> ImportActualAsync(Guid communityId,int year,int month,string accountCode,decimal amount,string sourceType,string sourceReference,CancellationToken ct)
    {
        if(await repository.ActualSourceExistsAsync(communityId,sourceType,sourceReference,ct))throw new InvalidOperationException("Actual source reference already imported.");var actual=BudgetActual.Create(communityId,year,month,accountCode,amount,sourceType,sourceReference);await repository.AddAsync(actual,ct);await repository.SaveChangesAsync(ct);return actual.Id;
    }

    public async Task<IReadOnlyList<BudgetVarianceRow>> GetVarianceAsync(Guid planId,CancellationToken ct)
    {
        var plan=await repository.GetPlanAsync(planId,ct)??throw new KeyNotFoundException("Budget plan not found.");var lines=await repository.GetLinesAsync(planId,ct);var actuals=await repository.GetActualsAsync(plan.CommunityId,plan.Year,ct);
        return lines.OrderBy(x=>x.Month).ThenBy(x=>x.AccountCode).Select(line=>{var actual=actuals.Where(x=>x.Month==line.Month&&x.AccountCode==line.AccountCode).Sum(x=>x.Amount);var variance=line.PlannedAmount-actual;var pct=line.PlannedAmount==0?(actual==0?0m:-100m):decimal.Round(variance/line.PlannedAmount*100m,2);return new BudgetVarianceRow(line.AccountCode,line.Category,line.Month,line.PlannedAmount,actual,variance,pct);}).ToArray();
    }

    public async Task<BudgetExecutiveSummary> GetSummaryAsync(Guid planId,int throughMonth,CancellationToken ct)
    {
        if(throughMonth is <1 or >12)throw new ArgumentOutOfRangeException(nameof(throughMonth));var rows=(await GetVarianceAsync(planId,ct)).Where(x=>x.Month<=throughMonth).ToArray();var planned=rows.Sum(x=>x.Planned);var actual=rows.Sum(x=>x.Actual);var variance=planned-actual;var execution=planned==0?(actual==0?0m:100m):decimal.Round(actual/planned*100m,2);return new(planned,actual,variance,execution,rows.Count(x=>x.Actual>x.Planned),rows.Count(x=>x.Actual<x.Planned));
    }
}
