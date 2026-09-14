using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using AppCondominio.Modules.Billing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class BillingPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync(); public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint07_persists_and_simulates_approved_charge_formula()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Billing",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();services.AddBillingModule(config);await using ServiceProvider provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<BillingDbContext>();await db.Database.MigrateAsync();}

        Guid communityId=Guid.NewGuid();Guid conceptId;Guid versionId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<BillingService>();
            conceptId=await service.CreateConceptAsync(communityId,"aliquota","Monthly fee",default);
            versionId=await service.CreateVersionAsync(communityId,conceptId,ChargeCalculationMethod.Coefficient,ChargePeriodicity.Monthly,new DateOnly(2026,1,1),0m,500m,0m,0m,default);
            await service.ApproveVersionAsync(versionId,"admin-demo",default);
            await service.MapAccountingAsync(communityId,conceptId,"4101","1102",null,"ADMIN",default);
            await service.AddInterestAsync(communityId,conceptId,1m,0m,5,30,50m,default);
            await service.AddDiscountAsync(communityId,conceptId,new DateOnly(2026,1,1),null,5m,0m,5,default);
            var result=await service.SimulateAsync(versionId,new(0,0,0.02m,0,0,0),new DateOnly(2026,9,1),new DateOnly(2026,9,10),0,default);
            Assert.Equal(10m,result.BaseAmount);Assert.Equal(0.50m,result.Discount);Assert.Equal(9.50m,result.Total);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            Assert.Single(await db.Concepts.Where(x=>x.CommunityId==communityId).ToArrayAsync());
            Assert.True((await db.Versions.SingleAsync(x=>x.Id==versionId)).IsApproved);
            Assert.Single(await db.AccountingMappings.Where(x=>x.ConceptId==conceptId).ToArrayAsync());
        }
    }
}
