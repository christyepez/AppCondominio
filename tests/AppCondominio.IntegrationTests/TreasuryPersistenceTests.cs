using AppCondominio.Modules.Treasury.Application;
using AppCondominio.Modules.Treasury.Domain;
using AppCondominio.Modules.Treasury.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
namespace AppCondominio.IntegrationTests;
public sealed class TreasuryPersistenceTests:IAsyncLifetime
{
 private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();public Task InitializeAsync()=>_sql.StartAsync();public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();
 [Fact] public async Task Sprint12_runs_payable_approval_payment_reverse_and_forecast(){IConfiguration cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Treasury",_sql.GetConnectionString()}}).Build();var services=new ServiceCollection();services.AddTreasuryModule(cfg);await using var provider=services.BuildServiceProvider();await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<TreasuryDbContext>();await db.Database.MigrateAsync();}var community=Guid.NewGuid();Guid payableId,requestId,disbursementId;await using(var scope=provider.CreateAsyncScope()){var s=scope.ServiceProvider.GetRequiredService<TreasuryService>();payableId=await s.CreatePayableAsync(community,Guid.NewGuid(),"INV-001",new DateOnly(2026,9,1),new DateOnly(2026,9,30),100m,"5101","2101",default);requestId=await s.RequestPaymentAsync(payableId,60m,"requester",default);await s.ApproveAsync(requestId,"manager",default);disbursementId=await s.PayAsync(requestId,Guid.NewGuid(),"PAY-001",new DateOnly(2026,9,8),default);var forecast=await s.ForecastAsync(community,default);Assert.Single(forecast);Assert.Equal(40m,forecast[0].Amount);await s.ReverseAsync(disbursementId,default);var forecast2=await s.ForecastAsync(community,default);Assert.Equal(100m,forecast2.Single().Amount);}await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<TreasuryDbContext>();Assert.Equal(100m,(await db.Payables.SingleAsync()).OutstandingAmount);Assert.True((await db.Disbursements.SingleAsync()).IsReversed);}}
}
