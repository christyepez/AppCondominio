using AppCondominio.Modules.Banking.Application;
using AppCondominio.Modules.Banking.Domain;
using AppCondominio.Modules.Banking.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class BankingPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync(); public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint10_import_is_idempotent_and_reconciliation_can_be_confirmed()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Banking",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();services.AddBankingModule(config);await using ServiceProvider provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<BankingDbContext>();await db.Database.MigrateAsync();}
        Guid communityId=Guid.NewGuid();Guid accountId;Guid paymentId=Guid.NewGuid();Guid transactionId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<BankingService>();accountId=await service.CreateAccountAsync(communityId,"PICHINCHA","123456","USD",default);
            var row=new ImportedBankTransaction("statement-1-row-1",new DateOnly(2026,9,8),100m,BankTransactionType.Credit,"PAY-001","deposit");
            Assert.Equal(1,await service.ImportAsync(communityId,accountId,[row],default));Assert.Equal(0,await service.ImportAsync(communityId,accountId,[row],default));
            var suggestions=await service.SuggestAsync(communityId,[new PaymentCandidate(paymentId,new DateOnly(2026,9,8),100m,"PAY-001",null)],0.80m,default);Assert.Single(suggestions);transactionId=suggestions[0].BankTransactionId;
            await service.MatchAsync(transactionId,paymentId,"treasurer",default);
        }
        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<BankingDbContext>();Assert.Single(await db.Transactions.ToArrayAsync());Assert.Equal(ReconciliationStatus.Matched,(await db.Reconciliations.SingleAsync()).Status);
        }
    }
}
