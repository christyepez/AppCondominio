using AppCondominio.Modules.Collections.Domain;

namespace AppCondominio.UnitTests;

public sealed class CollectionsDomainTests
{
    [Fact]
    public void Partial_payment_updates_receivable_and_payment_balances()
    {
        Guid communityId=Guid.NewGuid();
        var receivable=Receivable.Create(communityId,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),100m,new DateOnly(2026,9,1),new DateOnly(2026,9,10));
        var payment=Payment.Register(communityId,"TRX-001",PaymentMethod.BankTransfer,new DateOnly(2026,9,8),60m,"bank-001");
        payment.Apply(40m);receivable.Apply(40m);
        Assert.Equal(20m,payment.UnappliedAmount);Assert.Equal(PaymentStatus.PartiallyApplied,payment.Status);
        Assert.Equal(60m,receivable.OutstandingAmount);Assert.Equal(ReceivableStatus.PartiallyPaid,receivable.Status);
    }

    [Fact]
    public void Aging_bucket_changes_after_due_date()
    {
        var receivable=Receivable.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),50m,new DateOnly(2026,1,1),new DateOnly(2026,1,31));
        Assert.Equal(AgingBucket.Days31To60,receivable.GetAging(new DateOnly(2026,3,10)));
    }

    [Fact]
    public void Payment_cannot_apply_more_than_unapplied_balance()
    {
        var payment=Payment.Register(Guid.NewGuid(),"P-1",PaymentMethod.Cash,new DateOnly(2026,9,8),20m,null);
        Assert.Throws<InvalidOperationException>(()=>payment.Apply(25m));
    }
}
