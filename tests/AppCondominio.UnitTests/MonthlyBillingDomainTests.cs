using AppCondominio.Modules.Billing.Domain;

namespace AppCondominio.UnitTests;

public sealed class MonthlyBillingDomainTests
{
    [Fact]
    public void Billing_period_requires_draft_then_approval_before_issue()
    {
        var period=BillingPeriod.Open(Guid.NewGuid(),2026,9,new DateOnly(2026,9,1),new DateOnly(2026,9,10));
        Assert.Throws<InvalidOperationException>(()=>period.Issue(DateTimeOffset.UtcNow));
        period.MarkDrafted(2,125m);
        period.Approve("admin",DateTimeOffset.UtcNow);
        period.Issue(DateTimeOffset.UtcNow);
        Assert.Equal(BillingPeriodStatus.Issued,period.Status);
        Assert.Equal(2,period.DraftLineCount);
        Assert.Equal(125m,period.DraftTotal);
    }

    [Fact]
    public void Reversal_closes_obligation_without_negative_balance()
    {
        var period=BillingPeriod.Open(Guid.NewGuid(),2026,9,new DateOnly(2026,9,1),new DateOnly(2026,9,10));
        period.MarkDrafted(1,75m);period.Approve("admin",DateTimeOffset.UtcNow);
        var draft=DraftCharge.Create(period.CommunityId,period.Id,Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),75m,"Fixed");
        var obligation=ChargeObligation.Issue(period,draft);
        obligation.Reverse();
        Assert.Equal(0m,obligation.OutstandingAmount);
        Assert.Equal(ChargeObligationStatus.Reversed,obligation.Status);
    }

    [Fact]
    public void Period_rejects_invalid_due_date()
    {
        Assert.Throws<ArgumentException>(()=>BillingPeriod.Open(Guid.NewGuid(),2026,9,new DateOnly(2026,9,10),new DateOnly(2026,9,1)));
    }
}
