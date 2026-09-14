using AppCondominio.Modules.Banking.Domain;

namespace AppCondominio.Modules.Banking.Application;

public interface IBankingRepository
{
    Task AddAsync<T>(T entity,CancellationToken ct) where T:class;
    Task<bool> AccountExistsAsync(Guid communityId,string bankCode,string accountNumber,CancellationToken ct);
    Task<bool> ImportKeyExistsAsync(Guid bankAccountId,string importKey,CancellationToken ct);
    Task<BankTransaction?> GetTransactionAsync(Guid id,CancellationToken ct);
    Task<Reconciliation?> GetReconciliationByTransactionAsync(Guid transactionId,CancellationToken ct);
    Task<IReadOnlyList<BankTransaction>> ListUnmatchedCreditsAsync(Guid communityId,CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record ImportedBankTransaction(string ImportKey,DateOnly BookingDate,decimal Amount,BankTransactionType Type,string Reference,string Description);
public sealed record ReconciliationSuggestion(Guid BankTransactionId,Guid PaymentId,decimal Confidence,string Reason);

public sealed class BankingService(IBankingRepository repository)
{
    public async Task<Guid> CreateAccountAsync(Guid communityId,string bankCode,string accountNumber,string currency,CancellationToken ct)
    {
        if(await repository.AccountExistsAsync(communityId,bankCode.Trim(),accountNumber.Trim(),ct))throw new InvalidOperationException("Bank account already exists.");
        var account=BankAccount.Create(communityId,bankCode,accountNumber,currency);await repository.AddAsync(account,ct);await repository.SaveChangesAsync(ct);return account.Id;
    }

    public async Task<int> ImportAsync(Guid communityId,Guid bankAccountId,IReadOnlyCollection<ImportedBankTransaction> rows,CancellationToken ct)
    {
        int imported=0;
        foreach(var row in rows)
        {
            if(await repository.ImportKeyExistsAsync(bankAccountId,row.ImportKey,ct))continue;
            await repository.AddAsync(BankTransaction.Import(communityId,bankAccountId,row.ImportKey,row.BookingDate,row.Amount,row.Type,row.Reference,row.Description),ct);imported++;
        }
        await repository.SaveChangesAsync(ct);return imported;
    }

    public async Task<IReadOnlyList<ReconciliationSuggestion>> SuggestAsync(Guid communityId,IReadOnlyCollection<PaymentCandidate> payments,decimal minimumConfidence,CancellationToken ct)
    {
        if(minimumConfidence<0||minimumConfidence>1)throw new ArgumentOutOfRangeException(nameof(minimumConfidence));
        var result=new List<ReconciliationSuggestion>();
        foreach(var tx in await repository.ListUnmatchedCreditsAsync(communityId,ct))
        {
            var best=payments.Select(p=>(Payment:p,Score:ReconciliationScorer.Score(tx,p))).OrderByDescending(x=>x.Score).FirstOrDefault();
            if(best.Payment is null||best.Score<minimumConfidence)continue;
            var rec=await repository.GetReconciliationByTransactionAsync(tx.Id,ct)??Reconciliation.Create(communityId,tx.Id);
            if(rec.Id!=Guid.Empty&&rec.Status==ReconciliationStatus.Unmatched)await repository.AddAsync(rec,ct);
            rec.Suggest(best.Payment.PaymentId,best.Score,$"amount/date/reference score {best.Score:0.00}");result.Add(new(tx.Id,best.Payment.PaymentId,best.Score,rec.Reason??string.Empty));
        }
        await repository.SaveChangesAsync(ct);return result;
    }

    public async Task MatchAsync(Guid transactionId,Guid paymentId,string decidedBy,CancellationToken ct)
    {
        var tx=await repository.GetTransactionAsync(transactionId,ct)??throw new KeyNotFoundException("Bank transaction was not found.");
        var rec=await repository.GetReconciliationByTransactionAsync(transactionId,ct)??Reconciliation.Create(tx.CommunityId,tx.Id);
        if(rec.Status==ReconciliationStatus.Unmatched)await repository.AddAsync(rec,ct);rec.Match(paymentId,decidedBy);await repository.SaveChangesAsync(ct);
    }
}
