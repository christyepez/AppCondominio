using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Billing.Domain;

public enum ChargeCalculationMethod { Fixed=1, Area=2, Coefficient=3, Consumption=4, Percentage=5, Range=6, Proration=7, Manual=8 }
public enum ChargePeriodicity { OneTime=1, Monthly=2, Quarterly=3, SemiAnnual=4, Annual=5 }
public enum ChargeConceptStatus { Draft=0, Active=1, Inactive=2 }

public sealed class ChargeConcept:AggregateRoot
{
    private ChargeConcept(Guid id,Guid communityId,string code,string name):base(id){CommunityId=communityId;Code=code;Name=name;}
    public Guid CommunityId{get;private set;} public string Code{get;private set;} public string Name{get;private set;}
    public ChargeConceptStatus Status{get;private set;}=ChargeConceptStatus.Draft;
    public static ChargeConcept Create(Guid communityId,string code,string name)=>communityId==Guid.Empty?throw new ArgumentException("Community is required."):new(Guid.NewGuid(),communityId,Req(code).ToLowerInvariant(),Req(name));
    public void Activate()=>Status=ChargeConceptStatus.Active; public void Deactivate()=>Status=ChargeConceptStatus.Inactive;
    private static string Req(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("Required value missing."):v.Trim();
}

public sealed class ChargeConceptVersion:AggregateRoot
{
    private ChargeConceptVersion(Guid id,Guid communityId,Guid conceptId,int version,ChargeCalculationMethod method,ChargePeriodicity periodicity,DateOnly validFrom):base(id){CommunityId=communityId;ConceptId=conceptId;Version=version;Method=method;Periodicity=periodicity;ValidFrom=validFrom;}
    public Guid CommunityId{get;private set;} public Guid ConceptId{get;private set;} public int Version{get;private set;} public ChargeCalculationMethod Method{get;private set;} public ChargePeriodicity Periodicity{get;private set;} public DateOnly ValidFrom{get;private set;} public DateOnly? ValidTo{get;private set;}
    public decimal FixedAmount{get;private set;} public decimal Rate{get;private set;} public decimal Minimum{get;private set;} public decimal Maximum{get;private set;} public bool IsApproved{get;private set;} public string? ApprovedBy{get;private set;} public DateTimeOffset? ApprovedAtUtc{get;private set;}
    public static ChargeConceptVersion Create(Guid communityId,Guid conceptId,int version,ChargeCalculationMethod method,ChargePeriodicity periodicity,DateOnly validFrom,decimal fixedAmount,decimal rate,decimal minimum,decimal maximum)
    {
        if(communityId==Guid.Empty||conceptId==Guid.Empty)throw new ArgumentException("Community and concept are required."); if(version<1)throw new ArgumentOutOfRangeException(nameof(version)); if(fixedAmount<0||rate<0||minimum<0||maximum<0)throw new ArgumentOutOfRangeException(); if(maximum>0&&maximum<minimum)throw new ArgumentException("Maximum must be zero/unbounded or greater than minimum.");
        var x=new ChargeConceptVersion(Guid.NewGuid(),communityId,conceptId,version,method,periodicity,validFrom);x.FixedAmount=fixedAmount;x.Rate=rate;x.Minimum=minimum;x.Maximum=maximum;return x;
    }
    public void Approve(string approvedBy,DateTimeOffset atUtc){if(IsApproved)throw new InvalidOperationException("Version already approved.");ApprovedBy=string.IsNullOrWhiteSpace(approvedBy)?throw new ArgumentException("Approver is required."):approvedBy.Trim();ApprovedAtUtc=atUtc;IsApproved=true;}
    public void End(DateOnly validTo){if(validTo<ValidFrom)throw new ArgumentException("Invalid end date.");ValidTo=validTo;}
    public bool IsEffective(DateOnly on)=>IsApproved&&on>=ValidFrom&&(ValidTo is null||on<=ValidTo);
}

public sealed class AccountingMapping:AggregateRoot
{
    private AccountingMapping(Guid id,Guid communityId,Guid conceptId,string revenueAccount,string receivableAccount):base(id){CommunityId=communityId;ConceptId=conceptId;RevenueAccount=revenueAccount;ReceivableAccount=receivableAccount;}
    public Guid CommunityId{get;private set;} public Guid ConceptId{get;private set;} public string RevenueAccount{get;private set;} public string ReceivableAccount{get;private set;} public string? TaxAccount{get;private set;} public string? CostCenter{get;private set;}
    public static AccountingMapping Create(Guid communityId,Guid conceptId,string revenue,string receivable,string? tax,string? costCenter){var x=new AccountingMapping(Guid.NewGuid(),communityId,conceptId,Req(revenue),Req(receivable));x.TaxAccount=Opt(tax);x.CostCenter=Opt(costCenter);return x;}
    private static string Req(string v)=>string.IsNullOrWhiteSpace(v)?throw new ArgumentException("Required value missing."):v.Trim(); private static string? Opt(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
}

public sealed class InterestRule:AggregateRoot
{
    private InterestRule(Guid id,Guid communityId,Guid conceptId):base(id){CommunityId=communityId;ConceptId=conceptId;}
    public Guid CommunityId{get;private set;} public Guid ConceptId{get;private set;} public decimal Percentage{get;private set;} public decimal FixedAmount{get;private set;} public int GraceDays{get;private set;} public int EveryDays{get;private set;} public decimal MaximumAmount{get;private set;}
    public static InterestRule Create(Guid communityId,Guid conceptId,decimal percentage,decimal fixedAmount,int graceDays,int everyDays,decimal max){if(percentage<0||fixedAmount<0||graceDays<0||everyDays<1||max<0)throw new ArgumentOutOfRangeException();var x=new InterestRule(Guid.NewGuid(),communityId,conceptId);x.Percentage=percentage;x.FixedAmount=fixedAmount;x.GraceDays=graceDays;x.EveryDays=everyDays;x.MaximumAmount=max;return x;}
    public decimal Calculate(decimal outstanding,int daysLate){if(daysLate<=GraceDays||outstanding<=0)return 0;int periods=Math.Max(1,(int)Math.Ceiling((daysLate-GraceDays)/(decimal)EveryDays));decimal value=periods*(FixedAmount+outstanding*Percentage/100m);return MaximumAmount>0?Math.Min(value,MaximumAmount):value;}
}

public sealed class DiscountRule:AggregateRoot
{
    private DiscountRule(Guid id,Guid communityId,Guid conceptId,DateOnly validFrom):base(id){CommunityId=communityId;ConceptId=conceptId;ValidFrom=validFrom;}
    public Guid CommunityId{get;private set;} public Guid ConceptId{get;private set;} public DateOnly ValidFrom{get;private set;} public DateOnly? ValidTo{get;private set;} public decimal Percentage{get;private set;} public decimal FixedAmount{get;private set;} public int PayBeforeDueDays{get;private set;}
    public static DiscountRule Create(Guid communityId,Guid conceptId,DateOnly validFrom,DateOnly? validTo,decimal percentage,decimal fixedAmount,int payBeforeDueDays){if(percentage<0||percentage>100||fixedAmount<0||payBeforeDueDays<0)throw new ArgumentOutOfRangeException();if(validTo is not null&&validTo<validFrom)throw new ArgumentException("Invalid validity.");var x=new DiscountRule(Guid.NewGuid(),communityId,conceptId,validFrom);x.ValidTo=validTo;x.Percentage=percentage;x.FixedAmount=fixedAmount;x.PayBeforeDueDays=payBeforeDueDays;return x;}
    public decimal Calculate(decimal amount,DateOnly chargeDate,DateOnly dueDate){if(chargeDate<ValidFrom||(ValidTo is not null&&chargeDate>ValidTo))return 0;if(dueDate.DayNumber-chargeDate.DayNumber<PayBeforeDueDays)return 0;return Math.Min(amount,FixedAmount+amount*Percentage/100m);}
}

public sealed record ChargeSimulationInput(decimal Area,decimal TotalArea,decimal Coefficient,decimal Consumption,decimal PercentageBase,decimal ManualAmount,decimal ProrationFactor=1m);
public sealed record ChargeSimulationResult(decimal BaseAmount,decimal Discount,decimal Interest,decimal Total,string Formula);

public static class ChargeCalculator
{
    public static ChargeSimulationResult Simulate(ChargeConceptVersion version,ChargeSimulationInput input,DiscountRule? discount=null,InterestRule? interest=null,DateOnly? chargeDate=null,DateOnly? dueDate=null,int daysLate=0)
    {
        if(!version.IsApproved)throw new InvalidOperationException("Only approved versions can be simulated.");
        decimal raw=version.Method switch{ChargeCalculationMethod.Fixed=>version.FixedAmount,ChargeCalculationMethod.Area=>input.Area*version.Rate,ChargeCalculationMethod.Coefficient=>input.Coefficient*version.Rate,ChargeCalculationMethod.Consumption=>input.Consumption*version.Rate,ChargeCalculationMethod.Percentage=>input.PercentageBase*version.Rate/100m,ChargeCalculationMethod.Proration=>version.FixedAmount*input.ProrationFactor,ChargeCalculationMethod.Manual=>input.ManualAmount,ChargeCalculationMethod.Range=>input.Consumption*version.Rate,_=>throw new NotSupportedException()};
        decimal bounded=Math.Max(version.Minimum,version.Maximum>0?Math.Min(raw,version.Maximum):raw);decimal d=discount is null||chargeDate is null||dueDate is null?0:discount.Calculate(bounded,chargeDate.Value,dueDate.Value);decimal after=Math.Max(0,bounded-d);decimal i=interest?.Calculate(after,daysLate)??0;return new(decimal.Round(bounded,2),decimal.Round(d,2),decimal.Round(i,2),decimal.Round(after+i,2),version.Method.ToString());
    }
}
