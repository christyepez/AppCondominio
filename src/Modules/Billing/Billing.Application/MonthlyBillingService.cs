using AppCondominio.Contracts.Messaging;
using AppCondominio.Modules.Billing.Domain;

namespace AppCondominio.Modules.Billing.Application;

public sealed record DraftCandidateInput(Guid UnitId,Guid ResponsiblePersonId,Guid VersionId,ChargeSimulationInput Input);

public interface IMonthlyBillingRepository
{
    Task<BillingPeriod?> GetPeriodAsync(Guid id,CancellationToken ct);
    Task<bool> PeriodExistsAsync(Guid communityId,int year,int month,CancellationToken ct);
    Task<IReadOnlyList<BillingPeriod>> ListApprovedDueForIssueAsync(DateOnly on,CancellationToken ct);
    Task<ChargeConceptVersion?> GetVersionAsync(Guid id,CancellationToken ct);
    Task<DiscountRule?> GetDiscountAsync(Guid communityId,Guid conceptId,DateOnly on,CancellationToken ct);
    Task<InterestRule?> GetInterestAsync(Guid communityId,Guid conceptId,CancellationToken ct);
    Task<IReadOnlyList<DraftCharge>> GetDraftsAsync(Guid periodId,CancellationToken ct);
    Task<IReadOnlyList<BillingGenerationIssue>> GetIssuesAsync(Guid periodId,CancellationToken ct);
    Task<IReadOnlyList<ChargeObligation>> GetObligationsAsync(Guid periodId,CancellationToken ct);
    Task<IReadOnlyList<ChargeObligation>> GetUnitObligationsAsync(Guid communityId,Guid unitId,CancellationToken ct);
    Task DeleteDraftsAndIssuesAsync(Guid periodId,CancellationToken ct);
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class MonthlyBillingService(IMonthlyBillingRepository repository,IIntegrationEventPublisher publisher)
{
    public async Task<Guid> OpenPeriodAsync(Guid communityId,int year,int month,DateOnly issueDate,DateOnly dueDate,CancellationToken ct)
    {
        if(await repository.PeriodExistsAsync(communityId,year,month,ct))throw new InvalidOperationException("Billing period already exists.");
        var period=BillingPeriod.Open(communityId,year,month,issueDate,dueDate);await repository.AddAsync(period,ct);await repository.SaveChangesAsync(ct);return period.Id;
    }

    public async Task<DraftGenerationResult> GenerateDraftAsync(Guid periodId,IReadOnlyCollection<DraftCandidateInput> candidates,CancellationToken ct)
    {
        var period=await repository.GetPeriodAsync(periodId,ct)??throw new KeyNotFoundException("Billing period was not found.");
        if(period.Status is BillingPeriodStatus.Issued or BillingPeriodStatus.Reversed)throw new InvalidOperationException("Closed period cannot be regenerated.");
        await repository.DeleteDraftsAndIssuesAsync(periodId,ct);
        var issues=new List<string>();var duplicateKeys=new HashSet<string>(StringComparer.Ordinal);var drafts=new List<DraftCharge>();
        foreach(var candidate in candidates)
        {
            if(candidate.UnitId==Guid.Empty){await AddIssue("UNIT_MISSING","Unit is required.",null);continue;}
            if(candidate.ResponsiblePersonId==Guid.Empty){await AddIssue("RESPONSIBLE_MISSING","Financial responsible person is required.",candidate.UnitId);continue;}
            var version=await repository.GetVersionAsync(candidate.VersionId,ct);
            if(version is null){await AddIssue("FORMULA_NOT_FOUND","Charge formula version was not found.",candidate.UnitId);continue;}
            if(version.CommunityId!=period.CommunityId){await AddIssue("COMMUNITY_MISMATCH","Charge formula does not belong to the period community.",candidate.UnitId);continue;}
            if(!version.IsEffective(period.IssueDate)){await AddIssue("FORMULA_NOT_EFFECTIVE","Charge formula is not approved/effective for the issue date.",candidate.UnitId);continue;}
            string key=$"{candidate.UnitId:N}:{version.ConceptId:N}";if(!duplicateKeys.Add(key)){await AddIssue("DUPLICATE_CHARGE","Duplicate unit/concept candidate.",candidate.UnitId);continue;}
            var discount=await repository.GetDiscountAsync(period.CommunityId,version.ConceptId,period.IssueDate,ct);var interest=await repository.GetInterestAsync(period.CommunityId,version.ConceptId,ct);
            var simulation=ChargeCalculator.Simulate(version,candidate.Input,discount,interest,period.IssueDate,period.DueDate,0);
            var draft=DraftCharge.Create(period.CommunityId,period.Id,candidate.UnitId,candidate.ResponsiblePersonId,version.ConceptId,version.Id,simulation.Total,$"{simulation.Formula};base={simulation.BaseAmount:0.00};discount={simulation.Discount:0.00}");drafts.Add(draft);await repository.AddAsync(draft,ct);
        }
        decimal total=drafts.Sum(x=>x.Amount);period.MarkDrafted(drafts.Count,total);await repository.SaveChangesAsync(ct);return new(drafts.Count,total,issues);

        async Task AddIssue(string code,string message,Guid? unitId){issues.Add($"{code}:{message}");await repository.AddAsync(BillingGenerationIssue.Create(period.CommunityId,period.Id,unitId,code,message),ct);}
    }

    public async Task ApproveAsync(Guid periodId,string approvedBy,CancellationToken ct)
    {
        var period=await repository.GetPeriodAsync(periodId,ct)??throw new KeyNotFoundException("Billing period was not found.");
        var issues=await repository.GetIssuesAsync(periodId,ct);if(issues.Count>0)throw new InvalidOperationException("Billing draft has unresolved inconsistencies.");
        var drafts=await repository.GetDraftsAsync(periodId,ct);if(drafts.Count==0)throw new InvalidOperationException("Billing draft has no charge lines.");period.Approve(approvedBy,DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);
    }

    public async Task<int> IssueAsync(Guid periodId,CancellationToken ct)
    {
        var period=await repository.GetPeriodAsync(periodId,ct)??throw new KeyNotFoundException("Billing period was not found.");
        var existing=await repository.GetObligationsAsync(periodId,ct);if(existing.Count>0)throw new InvalidOperationException("Period already has issued obligations.");
        var drafts=await repository.GetDraftsAsync(periodId,ct);if(drafts.Count==0)throw new InvalidOperationException("No draft charges exist.");
        foreach(var draft in drafts)await repository.AddAsync(ChargeObligation.Issue(period,draft),ct);period.Issue(DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);return drafts.Count;
    }

    public async Task<int> IssueApprovedDuePeriodsAsync(DateOnly on,CancellationToken ct)
    {
        int issued=0;
        foreach(var period in await repository.ListApprovedDueForIssueAsync(on,ct))
        {
            try{issued+=await IssueAsync(period.Id,ct);}catch(InvalidOperationException){/* another worker/request may have issued it */}
        }
        return issued;
    }

    public async Task ReverseAsync(Guid periodId,string reason,CancellationToken ct)
    {
        var period=await repository.GetPeriodAsync(periodId,ct)??throw new KeyNotFoundException("Billing period was not found.");var obligations=await repository.GetObligationsAsync(periodId,ct);
        if(obligations.Any(x=>x.Status is ChargeObligationStatus.PartiallyPaid or ChargeObligationStatus.Paid))throw new InvalidOperationException("A period with applied payments cannot be reversed by Billing; Collections must reverse payments first.");
        foreach(var obligation in obligations)obligation.Reverse();period.Reverse(reason,DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);
    }

    public async Task<AccountStatement> GetStatementAsync(Guid communityId,Guid unitId,CancellationToken ct)
    {
        var obligations=await repository.GetUnitObligationsAsync(communityId,unitId,ct);var active=obligations.Where(x=>x.Status!=ChargeObligationStatus.Reversed).OrderBy(x=>x.IssuedOn).ToArray();
        return new(communityId,unitId,active.Sum(x=>x.OriginalAmount),active.Sum(x=>x.OutstandingAmount),active.Select(x=>new AccountStatementLine(x.Id,x.IssuedOn,x.DueOn,x.ConceptId,x.OriginalAmount,x.OutstandingAmount,x.Status)).ToArray());
    }

    public async Task NotifyStatementAsync(Guid communityId,Guid unitId,Guid responsiblePersonId,int year,int month,CancellationToken ct)
    {
        var statement=await GetStatementAsync(communityId,unitId,ct);await publisher.PublishAsync(new BillingStatementAvailable(communityId,unitId,responsiblePersonId,year,month,statement.Outstanding,DateTimeOffset.UtcNow),ct);
    }
}
