using AppCondominio.Modules.Governance.Application;
using AppCondominio.Modules.Governance.Domain;
using AppCondominio.Modules.Governance.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class GovernancePersistenceTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync() => _sql.StartAsync();
    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint17_runs_assembly_voting_and_coexistence_case()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["ConnectionStrings:Governance"] = _sql.GetConnectionString()
        }).Build();

        var services = new ServiceCollection();
        services.AddGovernanceModule(configuration);
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<GovernanceDbContext>().Database.MigrateAsync();

        var communityId = Guid.NewGuid();
        Guid assemblyId;
        Guid caseId;
        await using (var scope = provider.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<GovernanceService>();
            assemblyId = await service.CreateAssemblyAsync(communityId, "Annual assembly", DateTimeOffset.UtcNow.AddDays(1), 60m, default);
            await service.OpenAssemblyAsync(assemblyId, 100, 65, default);
            var motionId = await service.AddMotionAsync(assemblyId, "Approve annual budget", default);
            await service.VoteAsync(motionId, VoteChoice.Yes, default);
            await service.VoteAsync(motionId, VoteChoice.Yes, default);
            await service.VoteAsync(motionId, VoteChoice.No, default);
            await service.CloseMotionAsync(motionId, default);
            await service.CloseAssemblyAsync(assemblyId, default);

            caseId = await service.ReportCaseAsync(communityId, Guid.NewGuid(), "Noise", "Repeated disturbance", "resident", default);
            await service.StartReviewAsync(caseId, default);
            await service.ResolveCaseAsync(caseId, "Penalty approved", 30m, "governance:case:001", default);

            var kpi = await service.GetKpiAsync(communityId, default);
            Assert.Equal(1, kpi.ApprovedMotions);
            Assert.Equal(30m, kpi.ResolvedPenalties);
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GovernanceDbContext>();
            Assert.Single(await db.Assemblies.ToArrayAsync());
            Assert.Single(await db.Motions.ToArrayAsync());
            var item = Assert.Single(await db.Cases.ToArrayAsync());
            Assert.Equal(CoexistenceCaseStatus.Resolved, item.Status);
            Assert.Equal("governance:case:001", item.BillingReference);
        }
    }
}
