using AppCondominio.Modules.Billing.Domain;

namespace AppCondominio.UnitTests;

public sealed class BillingDomainTests
{
    [Fact]
    public void Fixed_charge_applies_discount_and_interest()
    {
        Guid communityId=Guid.NewGuid();Guid conceptId=Guid.NewGuid();
        var version=ChargeConceptVersion.Create(communityId,conceptId,1,ChargeCalculationMethod.Fixed,ChargePeriodicity.Monthly,new DateOnly(2026,1,1),100m,0m,0m,0m);
        version.Approve("admin",DateTimeOffset.UtcNow);
        var discount=DiscountRule.Create(communityId,conceptId,new DateOnly(2026,1,1),null,10m,0m,5);
        var interest=InterestRule.Create(communityId,conceptId,2m,0m,0,30,0m);
        var result=ChargeCalculator.Simulate(version,new(0,0,0,0,0,0),discount,interest,new DateOnly(2026,1,1),new DateOnly(2026,1,10),31);
        Assert.Equal(100m,result.BaseAmount);Assert.Equal(10m,result.Discount);Assert.Equal(1.80m,result.Interest);Assert.Equal(91.80m,result.Total);
    }

    [Fact]
    public void Area_charge_respects_minimum_and_maximum()
    {
        var version=ChargeConceptVersion.Create(Guid.NewGuid(),Guid.NewGuid(),1,ChargeCalculationMethod.Area,ChargePeriodicity.Monthly,new DateOnly(2026,1,1),0m,2m,50m,120m);
        version.Approve("admin",DateTimeOffset.UtcNow);
        Assert.Equal(50m,ChargeCalculator.Simulate(version,new(10m,0,0,0,0,0)).Total);
        Assert.Equal(120m,ChargeCalculator.Simulate(version,new(100m,0,0,0,0,0)).Total);
    }

    [Fact]
    public void Unapproved_version_cannot_be_simulated()
    {
        var version=ChargeConceptVersion.Create(Guid.NewGuid(),Guid.NewGuid(),1,ChargeCalculationMethod.Fixed,ChargePeriodicity.Monthly,new DateOnly(2026,1,1),50m,0m,0m,0m);
        Assert.Throws<InvalidOperationException>(()=>ChargeCalculator.Simulate(version,new(0,0,0,0,0,0)));
    }
}
