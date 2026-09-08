using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.UnitTests;

public sealed class SaasDomainTests
{
    [Fact]
    public void Commercial_plan_normalizes_modules_and_limits()
    {
        var plan = CommercialPlan.Create("PRO", "Professional", 300, 50, 4096, 99.90m, "usd",
            BillingPeriod.Monthly, ["billing", "properties", "Billing"]);

        Assert.Equal("pro", plan.Code);
        Assert.Equal("USD", plan.Currency);
        Assert.Equal(2, plan.Modules.Count);
        Assert.True(plan.AllowsModule("BILLING"));
    }

    [Fact]
    public void Subscription_inherits_plan_entitlements_and_can_be_suspended()
    {
        var plan = CommercialPlan.Create("pilot", "Pilot", 300, 100, 2048, 0, "USD",
            BillingPeriod.Annual, ["communities", "properties"]);
        var subscription = Subscription.Create(Guid.NewGuid(), plan, new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero));

        Assert.True(subscription.AllowsModule("properties"));
        Assert.Equal(new DateTimeOffset(2027, 9, 8, 0, 0, 0, TimeSpan.Zero), subscription.RenewsAtUtc);

        subscription.Suspend();
        Assert.False(subscription.AllowsModule("properties"));
        Assert.Equal(SubscriptionStatus.Suspended, subscription.Status);
    }

    [Fact]
    public void Dedicated_database_requires_secret_reference_not_raw_empty_configuration()
    {
        var profile = TenantDatabaseProfile.Shared(Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => profile.Configure(TenantDatabaseStrategy.Dedicated, null, "Pending"));

        profile.Configure(TenantDatabaseStrategy.Dedicated, "kv://appcondominio/tenant-001-sql", "Pending");
        Assert.Equal("kv://appcondominio/tenant-001-sql", profile.ConnectionSecretReference);
    }

    [Fact]
    public void Organization_suspension_is_reversible_and_keeps_reason_until_reactivation()
    {
        var organization = Organization.Create("Administradora Demo", "1790000000001");
        organization.Suspend("Subscription overdue", DateTimeOffset.UtcNow);
        Assert.False(organization.IsActive);
        Assert.Equal("Subscription overdue", organization.SuspensionReason);

        organization.Reactivate();
        Assert.True(organization.IsActive);
        Assert.Null(organization.SuspensionReason);
    }
}
