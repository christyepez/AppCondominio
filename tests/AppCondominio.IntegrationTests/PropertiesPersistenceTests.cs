using AppCondominio.Modules.Properties.Domain;
using AppCondominio.Modules.Properties.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class PropertiesPersistenceTests : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private readonly MsSqlContainer _sql = new MsSqlBuilder(SqlServerImage).Build();

    public Task InitializeAsync() => _sql.StartAsync();
    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint05_schema_persists_300_units_and_exact_100_percent_aliquot_total()
    {
        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseSqlServer(_sql.GetConnectionString(), sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "properties"))
            .Options;

        await using var db = new PropertiesDbContext(options);
        await db.Database.MigrateAsync();

        Guid communityId = Guid.NewGuid();
        var apartment = PropertyUnitType.Create(communityId, "apartment", "Apartment");
        db.UnitTypes.Add(apartment);

        const int unitCount = 300;
        decimal standard = decimal.Round(100m / unitCount, 6, MidpointRounding.AwayFromZero);
        decimal accumulated = 0m;
        for (int i = 1; i <= unitCount; i++)
        {
            var unit = PropertyUnit.Create(communityId, apartment.Id, $"A-{i:000}", $"Tower A / Unit {i:000}", 80m + i % 5);
            db.Units.Add(unit);
            db.Areas.Add(PropertyArea.Create(communityId, unit.Id, PropertyAreaType.Main, unit.MainAreaM2, true, "Main area"));
            decimal value = i == unitCount ? decimal.Round(100m - accumulated, 6, MidpointRounding.AwayFromZero) : standard;
            accumulated += value;
            var aliquot = AliquotVersion.Create(communityId, unit.Id, AliquotCalculationMethod.FixedPercentage, value, new DateOnly(2026, 1, 1), "Initial cadastre", null);
            aliquot.Approve("integration-test", DateTimeOffset.UtcNow);
            db.Aliquots.Add(aliquot);
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(300, await db.Units.CountAsync(x => x.CommunityId == communityId));
        Assert.Equal(300, await db.Areas.CountAsync(x => x.CommunityId == communityId));
        Assert.Equal(300, await db.Aliquots.CountAsync(x => x.CommunityId == communityId));
        Assert.Equal(100m, await db.Aliquots.Where(x => x.CommunityId == communityId).SumAsync(x => x.Value));
    }
}
