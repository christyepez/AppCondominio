using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Billing.Domain;

public enum BillingPeriodStatus { Open=0, Drafted=1, Approved=2, Issued=3, Reversed=4 }
public enum ChargeObligationStatus { Open=0, PartiallyPaid=1, Paid=2, Reversed=3 }

public sealed class BillingPeriod:AggregateRoot
{
    private BillingPeriod(Guid id,Guid communityId,int year,int month,DateOnly issueDate,DateOnly dueDate):base(id){CommunityId=communityId;Year=year;Month=month;IssueDate=issueDate;DueDate=dueDate;}
    public Guid CommunityId{get;private set;} public int Year{get;private set;} public int Month{get;private set;} public DateOnly IssueDate{get;private set;} public DateOnly DueDate{get;private set;} public BillingPeriodStatus Status{get;private set;}
    public int DraftLineCount{get;private set;} public decimal DraftTotal{get;private set;} public DateTimeOffset? ApprovedAtUtc{get;private set;} public string? ApprovedBy{get;private set;} public DateTimeOffset? IssuedAtUtc{get;private set;} public DateTimeOffset? ReversedAtUtc{get;private set;} public string? ReversalReason{get;private set;}
    public static BillingPeriod Open(Guid communityId,int year,int month,DateOnly issueDate,DateOnly dueDate){if(communityId==Guid.Empty)throw new ArgumentException("Community is required.");if(month is<1 or>12||year<2000)throw new ArgumentOutOfRangeException();if(dueDate<issueDate)throw new ArgumentException("Due date cannot precede issue date.");return new(Guid.NewGuid(),communityId,year,month,issueDate,dueDate);}
    public void MarkDrafted(int lineCount,decimal total){if(Status is BillingPeriodStatus.Issued or BillingPeriodStatus.Reversed)throw new InvalidOperationException("Closed period cannot be drafted.");DraftLineCount=lineCount;DraftTotal=total;Status=BillingPeriodStatus.Drafted;ApprovedAtUtc=null;ApprovedBy=null;}
    public void Approve(string approvedBy,DateTimeOffset now){if(Status!=BillingPeriodStatus.Drafted)throw new InvalidOperationException("Only a drafted period can be approved.");ApprovedBy=string.IsNullOrWhiteSpace(approvedBy)?throw new ArgumentException("Approver is required."):approvedBy.Trim();ApprovedAtUtc=now;Status=BillingPeriodStatus.Approved;}
    public void Issue(DateTimeOffset now){if(Status!=BillingPeriodStatus.Approved)throw new InvalidOperationException("Only an approved period can be issued.");IssuedAtUtc=now;Status=BillingPeriodStatus.Issued;}
    public void Reverse(string reason,DateTimeOffset now){if(Status!=BillingPeriodStatus.Issued)throw new InvalidOperationException("Only an issued period can be reversed.");ReversalReason=string.IsNullOrWhiteSpace(reason)?throw new ArgumentException("Reason is required."):reason.Trim();ReversedAtUtc=now;Status=BillingPeriodStatus.Reversed;}
}

public sealed class DraftCharge:AggregateRoot
{
    private DraftCharge(Guid id,Guid communityId,Guid periodId,Guid unitId,Guid responsiblePersonId,Guid conceptId,Guid conceptVersionId,decimal amount,string calculationTrace):base(id){CommunityId=communityId;PeriodId=periodId;UnitId=unitId;ResponsiblePersonId=responsiblePersonId;ConceptId=conceptId;ConceptVersionId=conceptVersionId;Amount=amount;CalculationTrace=calculationTrace;}
    public Guid CommunityId{get;private set;} public Guid PeriodId{get;private set;} public Guid UnitId{get;private set;} public Guid ResponsiblePersonId{get;private set;} public Guid ConceptId{get;private set;} public Guid ConceptVersionId{get;private set;} public decimal Amount{get;private set;} public string CalculationTrace{get;private set;}
    public static DraftCharge Create(Guid communityId,Guid periodId,Guid unitId,Guid responsiblePersonId,Guid conceptId,Guid conceptVersionId,decimal amount,string trace){if(new[]{communityId,periodId,unitId,responsiblePersonId,conceptId,conceptVersionId}.Any(x=>x==Guid.Empty))throw new ArgumentException("Required identifiers are missing.");if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));return new(Guid.NewGuid(),communityId,periodId,unitId,responsiblePersonId,conceptId,conceptVersionId,decimal.Round(amount,2),trace.Trim());}
}

public sealed class BillingGenerationIssue:AggregateRoot
{
    private BillingGenerationIssue(Guid id,Guid communityId,Guid periodId,Guid? unitId,string code,string message):base(id){CommunityId=communityId;PeriodId=periodId;UnitId=unitId;Code=code;Message=message;}
    public Guid CommunityId{get;private set;} public Guid PeriodId{get;private set;} public Guid? UnitId{get;private set;} public string Code{get;private set;} public string Message{get;private set;}
    public static BillingGenerationIssue Create(Guid communityId,Guid periodId,Guid? unitId,string code,string message)=>new(Guid.NewGuid(),communityId,periodId,unitId,code.Trim(),message.Trim());
}

public sealed class ChargeObligation:AggregateRoot
{
    private ChargeObligation(Guid id,Guid communityId,Guid periodId,Guid draftChargeId,Guid unitId,Guid responsiblePersonId,Guid conceptId,decimal originalAmount,DateOnly issuedOn,DateOnly dueOn):base(id){CommunityId=communityId;PeriodId=periodId;DraftChargeId=draftChargeId;UnitId=unitId;ResponsiblePersonId=responsiblePersonId;ConceptId=conceptId;OriginalAmount=originalAmount;OutstandingAmount=originalAmount;IssuedOn=issuedOn;DueOn=dueOn;}
    public Guid CommunityId{get;private set;} public Guid PeriodId{get;private set;} public Guid DraftChargeId{get;private set;} public Guid UnitId{get;private set;} public Guid ResponsiblePersonId{get;private set;} public Guid ConceptId{get;private set;} public decimal OriginalAmount{get;private set;} public decimal OutstandingAmount{get;private set;} public DateOnly IssuedOn{get;private set;} public DateOnly DueOn{get;private set;} public ChargeObligationStatus Status{get;private set;}
    public static ChargeObligation Issue(BillingPeriod period,DraftCharge draft){if(period.Id!=draft.PeriodId||period.CommunityId!=draft.CommunityId)throw new InvalidOperationException("Draft does not belong to period.");return new(Guid.NewGuid(),period.CommunityId,period.Id,draft.Id,draft.UnitId,draft.ResponsiblePersonId,draft.ConceptId,draft.Amount,period.IssueDate,period.DueDate);}
    public void Reverse(){OutstandingAmount=0;Status=ChargeObligationStatus.Reversed;}
}

public sealed record ChargeCandidate(Guid UnitId,Guid ResponsiblePersonId,Guid ConceptId,Guid ConceptVersionId,decimal Amount,string CalculationTrace);
public sealed record DraftGenerationResult(int Lines,decimal Total,IReadOnlyList<string> Issues);
public sealed record AccountStatementLine(Guid ObligationId,DateOnly IssuedOn,DateOnly DueOn,Guid ConceptId,decimal OriginalAmount,decimal OutstandingAmount,ChargeObligationStatus Status);
public sealed record AccountStatement(Guid CommunityId,Guid UnitId,decimal TotalCharged,decimal Outstanding,IReadOnlyList<AccountStatementLine> Lines);
