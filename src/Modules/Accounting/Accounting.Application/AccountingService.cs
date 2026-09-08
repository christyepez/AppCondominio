using AppCondominio.Modules.Accounting.Domain;

namespace AppCondominio.Modules.Accounting.Application;

public sealed record JournalLineInput(Guid AccountId,decimal Debit,decimal Credit,string? CostCenter=null);
public sealed record JournalView(Guid Id,Guid CommunityId,Guid PeriodId,DateOnly EntryDate,string Description,string SourceType,string SourceReference,JournalStatus Status,decimal Debit,decimal Credit);

public interface IAccountingRepository
{
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<LedgerAccount?> GetAccountAsync(Guid id,CancellationToken ct);
    Task<AccountingPeriod?> GetPeriodAsync(Guid id,CancellationToken ct);
    Task<bool> PeriodExistsAsync(Guid communityId,int year,int month,CancellationToken ct);
    Task<bool> SourceExistsAsync(Guid communityId,string sourceType,string sourceReference,CancellationToken ct);
    Task<JournalEntry?> GetJournalAsync(Guid id,CancellationToken ct);
    Task<IReadOnlyList<JournalLine>> GetJournalLinesAsync(Guid journalId,CancellationToken ct);
    Task<IReadOnlyList<(JournalEntry Journal,JournalLine Line,LedgerAccount Account)>> GetPostedLinesAsync(Guid communityId,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class AccountingService(IAccountingRepository repository)
{
    public async Task<Guid> CreateAccountAsync(Guid communityId,string code,string name,AccountType type,bool allowsPosting,CancellationToken ct){var x=LedgerAccount.Create(communityId,code,name,type,allowsPosting);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task<Guid> OpenPeriodAsync(Guid communityId,int year,int month,CancellationToken ct){if(await repository.PeriodExistsAsync(communityId,year,month,ct))throw new InvalidOperationException("Accounting period already exists.");var x=AccountingPeriod.Open(communityId,year,month);await repository.AddAsync(x,ct);await repository.SaveChangesAsync(ct);return x.Id;}
    public async Task ClosePeriodAsync(Guid id,string closedBy,CancellationToken ct){var p=await repository.GetPeriodAsync(id,ct)??throw new KeyNotFoundException("Accounting period was not found.");p.Close(closedBy,DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);}

    public async Task<Guid> PostAsync(Guid communityId,Guid periodId,DateOnly entryDate,string description,string sourceType,string sourceReference,IReadOnlyCollection<JournalLineInput> inputs,string postedBy,CancellationToken ct)
    {
        var period=await repository.GetPeriodAsync(periodId,ct)??throw new KeyNotFoundException("Accounting period was not found.");
        if(period.CommunityId!=communityId||period.Status!=AccountingPeriodStatus.Open)throw new InvalidOperationException("Accounting period is not open for this community.");
        if(period.Year!=entryDate.Year||period.Month!=entryDate.Month)throw new InvalidOperationException("Entry date is outside the accounting period.");
        if(await repository.SourceExistsAsync(communityId,sourceType,sourceReference,ct))throw new InvalidOperationException("Accounting source reference already posted.");
        if(inputs.Count<2)throw new InvalidOperationException("Journal requires at least two lines.");
        var journal=JournalEntry.Create(communityId,periodId,entryDate,description,sourceType,sourceReference);await repository.AddAsync(journal,ct);
        decimal debit=0,credit=0;
        foreach(var input in inputs)
        {
            var account=await repository.GetAccountAsync(input.AccountId,ct)??throw new KeyNotFoundException("Ledger account was not found.");
            if(account.CommunityId!=communityId||!account.IsActive||!account.AllowsPosting)throw new InvalidOperationException("Ledger account is not available for posting.");
            var line=JournalLine.Create(communityId,journal.Id,account.Id,account.Code,input.Debit,input.Credit,input.CostCenter);debit+=line.Debit;credit+=line.Credit;await repository.AddAsync(line,ct);
        }
        journal.Post(decimal.Round(debit,2),decimal.Round(credit,2),postedBy,DateTimeOffset.UtcNow);await repository.SaveChangesAsync(ct);return journal.Id;
    }

    public async Task ReverseAsync(Guid journalId,string reversedBy,CancellationToken ct)
    {
        var original=await repository.GetJournalAsync(journalId,ct)??throw new KeyNotFoundException("Journal was not found.");if(original.Status!=JournalStatus.Posted)throw new InvalidOperationException("Journal is not posted.");
        var period=await repository.GetPeriodAsync(original.PeriodId,ct)??throw new InvalidOperationException("Accounting period is missing.");if(period.Status!=AccountingPeriodStatus.Open)throw new InvalidOperationException("Closed-period journals cannot be reversed in place.");
        var lines=await repository.GetJournalLinesAsync(journalId,ct);var reversal=JournalEntry.Create(original.CommunityId,original.PeriodId,original.EntryDate,$"REVERSAL: {original.Description}","reversal",original.Id.ToString("N"));await repository.AddAsync(reversal,ct);
        foreach(var line in lines)await repository.AddAsync(JournalLine.Create(line.CommunityId,reversal.Id,line.AccountId,line.AccountCode,line.Credit,line.Debit,line.CostCenter),ct);
        reversal.Post(lines.Sum(x=>x.Credit),lines.Sum(x=>x.Debit),reversedBy,DateTimeOffset.UtcNow);original.Reverse();await repository.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(Guid communityId,CancellationToken ct)
    {
        var rows=await repository.GetPostedLinesAsync(communityId,ct);return rows.GroupBy(x=>new{x.Account.Code,x.Account.Name,x.Account.Type}).OrderBy(x=>x.Key.Code).Select(g=>{var d=g.Sum(x=>x.Line.Debit);var c=g.Sum(x=>x.Line.Credit);return new TrialBalanceRow(g.Key.Code,g.Key.Name,g.Key.Type,d,c,d-c);}).ToArray();
    }

    public async Task<IReadOnlyList<GeneralLedgerLine>> GetLedgerAsync(Guid communityId,Guid accountId,CancellationToken ct)
    {
        var rows=(await repository.GetPostedLinesAsync(communityId,ct)).Where(x=>x.Account.Id==accountId).OrderBy(x=>x.Journal.EntryDate).ThenBy(x=>x.Journal.Id).ToArray();decimal running=0;var result=new List<GeneralLedgerLine>();foreach(var x in rows){running+=x.Line.Debit-x.Line.Credit;result.Add(new(x.Journal.EntryDate,x.Journal.Description,x.Journal.SourceType,x.Journal.SourceReference,x.Line.Debit,x.Line.Credit,running));}return result;
    }
}
