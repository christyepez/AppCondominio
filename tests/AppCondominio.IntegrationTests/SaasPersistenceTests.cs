using AppCondominio.Modules.Organizations.Domain;
using AppCondominio.Modules.Organizations.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class SaasPersistenceTests
{
    [Fact]
    public async Task Sprint03_schema_persists_plan_subscription_branding_and_database_profile()
    {
        await using var sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await sql.StartAsync();

        var options = new DbContextOptionsBuilder<OrganizationsDbContext>()
            .UseSqlServer(sql.GetConnectionString(), x => x.MigrationsHistoryTable("__EFMigrationsHistory", "organizations"))
            .Options;

        await using var db = new OrganizationsDbContext(options);
        await db.Database.MigrateAsync();

        var organization = Organization.Create("Administradora Piloto", "1790000000001");
        var plan = CommercialPlan.Create("pilot-300", "Pilot 300", 300, 100, 4096, 99m, "USD",
            BillingPeriod.Monthly, ["communities", "properties", "billing"]);
        var subscription = Subscription.Create(organization.Id, plan, DateTimeOffset.UtcNow);
        var branding = BrandSettings.Create(organization.Id, "Mi Condominio");
        branding.Update("Mi Condominio", null, null, "#3C235F", "#F28C28", "admin@example.com", null);
        var database = TenantDatabaseProfile.Shared(organization.Id);

        db.AddRange(organization, plan, subscription, branding, database);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.NotNull(await db.Subscriptions.SingleOrDefaultAsync(x => x.OrganizationId == organization.Id));
        Assert.Equal("pilot-300", (await db.CommercialPlans.SingleAsync()).Code);
        Assert.Equal("Mi Condominio", (await db.BrandSettings.SingleAsync()).ProductName);
        Assert.Equal(TenantDatabaseStrategy.Shared, (await db.TenantDatabaseProfiles.SingleAsync()).Strategy);
    }
}
