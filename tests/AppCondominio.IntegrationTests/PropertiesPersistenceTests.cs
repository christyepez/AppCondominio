using AppCondominio.Modules.Properties.Application;
using AppCondominio.Modules.Properties.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class PropertiesPersistenceTests : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private readonly MsSqlContainer _sql = new MsSqlBuilder(SqlServerImage).Build();

    public Task InitializeAsync() => _sql.StartAsync();
    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint05_demo_service_generates_300_units_with_exact_100_percent_total()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Properties"] = _sql.GetConnectionString()
            })
            .Build();
        var services = new ServiceCollection();
        services.AddPropertiesModule(configuration);
        await using ServiceProvider provider = services.BuildServiceProvider();

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
            await db.Database.MigrateAsync();
        }

        Guid communityId = Guid.NewGuid();
        DemoGenerationResult result;
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<PropertyService>();
            result = await service.GenerateDemoAsync(communityId, 300, CancellationToken.None);
        }

        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
            Assert.Equal(300, await db.Units.CountAsync(x => x.CommunityId == communityId));
            Assert.Equal(300, await db.Areas.CountAsync(x => x.CommunityId == communityId));
            Assert.Equal(300, await db.Aliquots.CountAsync(x => x.CommunityId == communityId));
            Assert.Equal(3, await db.UnitTypes.CountAsync(x => x.CommunityId == communityId));
            Assert.Equal(100m, await db.Aliquots.Where(x => x.CommunityId == communityId).SumAsync(x => x.Value));
        }

        Assert.Equal(300, result.UnitsCreated);
        Assert.Equal(3, result.TypesCreated);
        Assert.Equal(100m, result.AliquotTotal);
    }
}
