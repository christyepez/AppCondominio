using AppCondominio.Modules.Accounting.Domain;

namespace AppCondominio.UnitTests;

public sealed class AccountingDomainTests
{
    [Fact]
    public void Journal_cannot_post_when_not_balanced()
    {
        var journal=JournalEntry.Create(Guid.NewGuid(),Guid.NewGuid(),new DateOnly(2026,9,1),"Monthly billing","billing","period-2026-09");
        Assert.Throws<InvalidOperationException>(()=>journal.Post(100m,99m,"accountant",DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Journal_line_requires_exactly_one_side()
    {
        Assert.Throws<ArgumentException>(()=>JournalLine.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"1101",10m,10m));
        Assert.Throws<ArgumentException>(()=>JournalLine.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"1101",0m,0m));
    }

    [Fact]
    public void Closed_period_cannot_be_closed_twice()
    {
        var p=AccountingPeriod.Open(Guid.NewGuid(),2026,9);p.Close("admin",DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(()=>p.Close("admin",DateTimeOffset.UtcNow));
    }
}
