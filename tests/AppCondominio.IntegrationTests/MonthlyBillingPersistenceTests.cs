using AppCondominio.Contracts.Messaging;
using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using AppCondominio.Modules.Billing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class MonthlyBillingPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync(); public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint08_generates_approves_issues_and_reads_statement()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Billing",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();services.AddSingleton<IIntegrationEventPublisher,TestPublisher>();services.AddBillingModule(config);await using ServiceProvider provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<BillingDbContext>();await db.Database.MigrateAsync();}

        Guid communityId=Guid.NewGuid();Guid unitId=Guid.NewGuid();Guid responsibleId=Guid.NewGuid();Guid versionId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var billing=scope.ServiceProvider.GetRequiredService<BillingService>();
            Guid conceptId=await billing.CreateConceptAsync(communityId,"monthly-fee","Monthly fee",default);
            versionId=await billing.CreateVersionAsync(communityId,conceptId,ChargeCalculationMethod.Fixed,ChargePeriodicity.Monthly,new DateOnly(2026,1,1),100m,0m,0m,0m,default);
            await billing.ApproveVersionAsync(versionId,"admin",default);
        }

        Guid periodId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var monthly=scope.ServiceProvider.GetRequiredService<MonthlyBillingService>();
            periodId=await monthly.OpenPeriodAsync(communityId,2026,9,new DateOnly(2026,9,1),new DateOnly(2026,9,10),default);
            var draft=await monthly.GenerateDraftAsync(periodId,[new DraftCandidateInput(unitId,responsibleId,versionId,new(0,0,0,0,0,0))],default);
            Assert.Equal(1,draft.Lines);Assert.Equal(100m,draft.Total);Assert.Empty(draft.Issues);
            await monthly.ApproveAsync(periodId,"admin",default);
            Assert.Equal(1,await monthly.IssueAsync(periodId,default));
            var statement=await monthly.GetStatementAsync(communityId,unitId,default);
            Assert.Equal(100m,statement.Outstanding);Assert.Single(statement.Lines);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            Assert.Equal(BillingPeriodStatus.Issued,(await db.BillingPeriods.SingleAsync(x=>x.Id==periodId)).Status);
            Assert.Single(await db.ChargeObligations.Where(x=>x.PeriodId==periodId).ToArrayAsync());
        }
    }

    private sealed class TestPublisher:IIntegrationEventPublisher
    {
        public Task PublishAsync<T>(T message,CancellationToken cancellationToken=default) where T:class=>Task.CompletedTask;
    }
}
