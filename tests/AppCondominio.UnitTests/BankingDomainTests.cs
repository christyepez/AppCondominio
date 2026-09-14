using AppCondominio.Modules.Banking.Domain;

namespace AppCondominio.UnitTests;

public sealed class BankingDomainTests
{
    [Fact]
    public void Exact_amount_same_day_and_reference_scores_full_confidence()
    {
        var tx=BankTransaction.Import(Guid.NewGuid(),Guid.NewGuid(),"row-1",new DateOnly(2026,9,8),100m,BankTransactionType.Credit,"BANK-001 payment-123","deposit");
        var payment=new PaymentCandidate(Guid.NewGuid(),new DateOnly(2026,9,8),100m,"payment-123",null);
        Assert.Equal(1m,ReconciliationScorer.Score(tx,payment));
    }

    [Fact]
    public void Suggestion_confidence_is_bounded()
    {
        var rec=Reconciliation.Create(Guid.NewGuid(),Guid.NewGuid());
        Assert.Throws<ArgumentException>(()=>rec.Suggest(Guid.NewGuid(),1.1m,"invalid"));
    }
}
