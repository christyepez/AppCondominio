using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Accounting.Domain;

public enum AccountType{Asset=0,Liability=1,Equity=2,Revenue=3,Expense=4}
public enum AccountingPeriodStatus{Open=0,Closed=1}
public enum JournalStatus{Draft=0,Posted=1,Reversed=2}

public sealed class LedgerAccount:AggregateRoot
{
    private LedgerAccount(Guid id,Guid communityId,string code,string name,AccountType type,bool allowsPosting):base(id){CommunityId=communityId;Code=code;Name=name;Type=type;AllowsPosting=allowsPosting;IsActive=true;}
    public Guid CommunityId{get;private set;} public string Code{get;private set;} public string Name{get;private set;} public AccountType Type{get;private set;} public bool AllowsPosting{get;private set;} public bool IsActive{get;private set;}
    public static LedgerAccount Create(Guid communityId,string code,string name,AccountType type,bool allowsPosting=true){if(communityId==Guid.Empty)throw new ArgumentException("Community is required.");return new(Guid.NewGuid(),communityId,Req(code),Req(name),type,allowsPosting);}
    public void Deactivate()=>IsActive=false;
    private static string Req(string value)=>string.IsNullOrWhiteSpace(value)?throw new ArgumentException("Required value missing."):value.Trim();
}

public sealed class AccountingPeriod:AggregateRoot
{
    private AccountingPeriod(Guid id,Guid communityId,int year,int month):base(id){CommunityId=communityId;Year=year;Month=month;}
    public Guid CommunityId{get;private set;} public int Year{get;private set;} public int Month{get;private set;} public AccountingPeriodStatus Status{get;private set;} public DateTimeOffset? ClosedAtUtc{get;private set;} public string? ClosedBy{get;private set;}
    public static AccountingPeriod Open(Guid communityId,int year,int month){if(communityId==Guid.Empty)throw new ArgumentException("Community is required.");if(year<2000||month is<1 or>12)throw new ArgumentOutOfRangeException();return new(Guid.NewGuid(),communityId,year,month);}
    public void Close(string closedBy,DateTimeOffset now){if(Status==AccountingPeriodStatus.Closed)throw new InvalidOperationException("Period already closed.");ClosedBy=string.IsNullOrWhiteSpace(closedBy)?throw new ArgumentException("Closer is required."):closedBy.Trim();ClosedAtUtc=now;Status=AccountingPeriodStatus.Closed;}
}

public sealed class JournalEntry:AggregateRoot
{
    private JournalEntry(Guid id,Guid communityId,Guid periodId,DateOnly entryDate,string description,string sourceType,string sourceReference):base(id){CommunityId=communityId;PeriodId=periodId;EntryDate=entryDate;Description=description;SourceType=sourceType;SourceReference=sourceReference;}
    public Guid CommunityId{get;private set;} public Guid PeriodId{get;private set;} public DateOnly EntryDate{get;private set;} public string Description{get;private set;} public string SourceType{get;private set;} public string SourceReference{get;private set;} public JournalStatus Status{get;private set;} public DateTimeOffset? PostedAtUtc{get;private set;} public string? PostedBy{get;private set;}
    public static JournalEntry Create(Guid communityId,Guid periodId,DateOnly entryDate,string description,string sourceType,string sourceReference){if(communityId==Guid.Empty||periodId==Guid.Empty)throw new ArgumentException("Community and period are required.");return new(Guid.NewGuid(),communityId,periodId,entryDate,Req(description),Req(sourceType),Req(sourceReference));}
    public void Post(decimal debit,decimal credit,string postedBy,DateTimeOffset now){if(Status!=JournalStatus.Draft)throw new InvalidOperationException("Only draft journals can be posted.");if(debit<=0||credit<=0||debit!=credit)throw new InvalidOperationException("Journal must be balanced and non-zero.");PostedBy=Req(postedBy);PostedAtUtc=now;Status=JournalStatus.Posted;}
    public void Reverse(){if(Status!=JournalStatus.Posted)throw new InvalidOperationException("Only posted journals can be reversed.");Status=JournalStatus.Reversed;}
    private static string Req(string value)=>string.IsNullOrWhiteSpace(value)?throw new ArgumentException("Required value missing."):value.Trim();
}

public sealed class JournalLine:AggregateRoot
{
    private JournalLine(Guid id,Guid communityId,Guid journalEntryId,Guid accountId,string accountCode,decimal debit,decimal credit,string? costCenter):base(id){CommunityId=communityId;JournalEntryId=journalEntryId;AccountId=accountId;AccountCode=accountCode;Debit=debit;Credit=credit;CostCenter=costCenter;}
    public Guid CommunityId{get;private set;} public Guid JournalEntryId{get;private set;} public Guid AccountId{get;private set;} public string AccountCode{get;private set;} public decimal Debit{get;private set;} public decimal Credit{get;private set;} public string? CostCenter{get;private set;}
    public static JournalLine Create(Guid communityId,Guid journalEntryId,Guid accountId,string accountCode,decimal debit,decimal credit,string? costCenter=null){if(new[]{communityId,journalEntryId,accountId}.Any(x=>x==Guid.Empty))throw new ArgumentException("Required identifiers are missing.");if(debit<0||credit<0||debit==credit||debit+credit<=0)throw new ArgumentException("A journal line must contain exactly one positive debit or credit amount.");return new(Guid.NewGuid(),communityId,journalEntryId,accountId,accountCode.Trim(),decimal.Round(debit,2),decimal.Round(credit,2),string.IsNullOrWhiteSpace(costCenter)?null:costCenter.Trim());}
}

public sealed record TrialBalanceRow(string AccountCode,string AccountName,AccountType AccountType,decimal Debit,decimal Credit,decimal Balance);
public sealed record GeneralLedgerLine(DateOnly EntryDate,string JournalDescription,string SourceType,string SourceReference,decimal Debit,decimal Credit,decimal RunningBalance);
