using AppCondominio.Modules.Billing.Domain;

namespace AppCondominio.Modules.Billing.Application;

public interface IBillingRepository
{
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<ChargeConcept?> GetConceptAsync(Guid id,CancellationToken ct);
    Task<ChargeConceptVersion?> GetVersionAsync(Guid id,CancellationToken ct);
    Task<DiscountRule?> GetDiscountAsync(Guid communityId,Guid conceptId,DateOnly on,CancellationToken ct);
    Task<InterestRule?> GetInterestAsync(Guid communityId,Guid conceptId,CancellationToken ct);
    Task<int> NextVersionAsync(Guid communityId,Guid conceptId,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class BillingService(IBillingRepository repository)
{
    public async Task<Guid> CreateConceptAsync(Guid communityId,string code,string name,CancellationToken ct){var x=ChargeConcept.Create(communityId,code,name);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> CreateVersionAsync(Guid communityId,Guid conceptId,ChargeCalculationMethod method,ChargePeriodicity periodicity,DateOnly validFrom,decimal fixedAmount,decimal rate,decimal minimum,decimal maximum,CancellationToken ct)
    {if(await repository.GetConceptAsync(conceptId,ct) is null)throw new KeyNotFoundException("Concept was not found.");int version=await repository.NextVersionAsync(communityId,conceptId,ct);var x=ChargeConceptVersion.Create(communityId,conceptId,version,method,periodicity,validFrom,fixedAmount,rate,minimum,maximum);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task ApproveVersionAsync(Guid versionId,string approvedBy,CancellationToken ct){var x=await repository.GetVersionAsync(versionId,ct)??throw new KeyNotFoundException("Version was not found.");x.Approve(approvedBy,DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);}
    public async Task<Guid> MapAccountingAsync(Guid communityId,Guid conceptId,string revenue,string receivable,string? tax,string? costCenter,CancellationToken ct){var x=AccountingMapping.Create(communityId,conceptId,revenue,receivable,tax,costCenter);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddInterestAsync(Guid communityId,Guid conceptId,decimal percentage,decimal fixedAmount,int graceDays,int everyDays,decimal max,CancellationToken ct){var x=InterestRule.Create(communityId,conceptId,percentage,fixedAmount,graceDays,everyDays,max);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> AddDiscountAsync(Guid communityId,Guid conceptId,DateOnly validFrom,DateOnly? validTo,decimal percentage,decimal fixedAmount,int payBeforeDueDays,CancellationToken ct){var x=DiscountRule.Create(communityId,conceptId,validFrom,validTo,percentage,fixedAmount,payBeforeDueDays);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<ChargeSimulationResult> SimulateAsync(Guid versionId,ChargeSimulationInput input,DateOnly chargeDate,DateOnly dueDate,int daysLate,CancellationToken ct)
    {var version=await repository.GetVersionAsync(versionId,ct)??throw new KeyNotFoundException("Version was not found.");var discount=await repository.GetDiscountAsync(version.CommunityId,version.ConceptId,chargeDate,ct);var interest=await repository.GetInterestAsync(version.CommunityId,version.ConceptId,ct);return ChargeCalculator.Simulate(version,input,discount,interest,chargeDate,dueDate,daysLate);}
}
