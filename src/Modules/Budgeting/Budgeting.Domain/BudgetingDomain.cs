using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Budgeting.Domain;

public enum BudgetStatus{Draft=0,Approved=1,Closed=2}

public sealed class BudgetPlan:AggregateRoot
{
    private BudgetPlan(Guid id,Guid communityId,int year,int version,string name):base(id){CommunityId=communityId;Year=year;Version=version;Name=name;CreatedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public int Year{get;private set;} public int Version{get;private set;} public string Name{get;private set;} public BudgetStatus Status{get;private set;} public DateTimeOffset CreatedAtUtc{get;private set;} public string? ApprovedBy{get;private set;} public DateTimeOffset? ApprovedAtUtc{get;private set;} public string? ClosedBy{get;private set;} public DateTimeOffset? ClosedAtUtc{get;private set;}
    public static BudgetPlan Create(Guid communityId,int year,int version,string name){if(communityId==Guid.Empty||year<2000||year>2200||version<=0)throw new ArgumentException("Invalid budget identity.");return new(Guid.NewGuid(),communityId,year,version,Req(name));}
    public void Approve(string by){if(Status!=BudgetStatus.Draft)throw new InvalidOperationException("Only draft budgets can be approved.");Status=BudgetStatus.Approved;ApprovedBy=Req(by);ApprovedAtUtc=DateTimeOffset.UtcNow;}
    public void Close(string by){if(Status!=BudgetStatus.Approved)throw new InvalidOperationException("Only approved budgets can be closed.");Status=BudgetStatus.Closed;ClosedBy=Req(by);ClosedAtUtc=DateTimeOffset.UtcNow;}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class BudgetLine:AggregateRoot
{
    private BudgetLine(Guid id,Guid budgetPlanId,Guid communityId,string accountCode,string category,int month,decimal plannedAmount):base(id){BudgetPlanId=budgetPlanId;CommunityId=communityId;AccountCode=accountCode;Category=category;Month=month;PlannedAmount=plannedAmount;}
    public Guid BudgetPlanId{get;private set;} public Guid CommunityId{get;private set;} public string AccountCode{get;private set;} public string Category{get;private set;} public int Month{get;private set;} public decimal PlannedAmount{get;private set;}
    public static BudgetLine Create(Guid budgetPlanId,Guid communityId,string accountCode,string category,int month,decimal plannedAmount){if(budgetPlanId==Guid.Empty||communityId==Guid.Empty||month is <1 or >12||plannedAmount<0)throw new ArgumentException("Invalid budget line.");return new(Guid.NewGuid(),budgetPlanId,communityId,Req(accountCode),Req(category),month,decimal.Round(plannedAmount,2));}
    public void ChangeAmount(decimal value){if(value<0)throw new ArgumentOutOfRangeException(nameof(value));PlannedAmount=decimal.Round(value,2);}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed class BudgetActual:AggregateRoot
{
    private BudgetActual(Guid id,Guid communityId,int year,int month,string accountCode,decimal amount,string sourceType,string sourceReference):base(id){CommunityId=communityId;Year=year;Month=month;AccountCode=accountCode;Amount=amount;SourceType=sourceType;SourceReference=sourceReference;ImportedAtUtc=DateTimeOffset.UtcNow;}
    public Guid CommunityId{get;private set;} public int Year{get;private set;} public int Month{get;private set;} public string AccountCode{get;private set;} public decimal Amount{get;private set;} public string SourceType{get;private set;} public string SourceReference{get;private set;} public DateTimeOffset ImportedAtUtc{get;private set;}
    public static BudgetActual Create(Guid communityId,int year,int month,string accountCode,decimal amount,string sourceType,string sourceReference){if(communityId==Guid.Empty||year<2000||month is <1 or >12)throw new ArgumentException("Invalid actual.");return new(Guid.NewGuid(),communityId,year,month,Req(accountCode),decimal.Round(amount,2),Req(sourceType),Req(sourceReference));}
    private static string Req(string x)=>string.IsNullOrWhiteSpace(x)?throw new ArgumentException("Required value missing."):x.Trim();
}

public sealed record BudgetVarianceRow(string AccountCode,string Category,int Month,decimal Planned,decimal Actual,decimal Variance,decimal VariancePercent);
public sealed record BudgetExecutiveSummary(decimal PlannedYtd,decimal ActualYtd,decimal VarianceYtd,decimal ExecutionPercent,int OverBudgetLines,int UnderBudgetLines);
