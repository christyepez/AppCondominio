using AppCondominio.Modules.Accounting.Application;
using AppCondominio.Modules.Accounting.Domain;
using AppCondominio.Modules.Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class AccountingPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync();public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint11_posts_balanced_journal_and_generates_trial_balance()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Accounting",_sql.GetConnectionString()}}).Build();var services=new ServiceCollection();services.AddAccountingModule(config);await using var provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<AccountingDbContext>();await db.Database.MigrateAsync();}
        var communityId=Guid.NewGuid();Guid cashId,revId,periodId,journalId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var s=scope.ServiceProvider.GetRequiredService<AccountingService>();cashId=await s.CreateAccountAsync(communityId,"1101","Cash",AccountType.Asset,true,default);revId=await s.CreateAccountAsync(communityId,"4101","Fees revenue",AccountType.Revenue,true,default);periodId=await s.OpenPeriodAsync(communityId,2026,9,default);journalId=await s.PostAsync(communityId,periodId,new DateOnly(2026,9,8),"Payment applied","collections","payment-001",[new(cashId,100m,0m),new(revId,0m,100m)],"accountant",default);
            var tb=await s.GetTrialBalanceAsync(communityId,default);Assert.Equal(2,tb.Count);Assert.Equal(100m,tb.Sum(x=>x.Debit));Assert.Equal(100m,tb.Sum(x=>x.Credit));
            var ledger=await s.GetLedgerAsync(communityId,cashId,default);Assert.Single(ledger);Assert.Equal(100m,ledger[0].RunningBalance);
            await s.ReverseAsync(journalId,"accountant",default);var tb2=await s.GetTrialBalanceAsync(communityId,default);Assert.Empty(tb2);
        }
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<AccountingDbContext>();Assert.Equal(2,await db.Journals.CountAsync());Assert.Equal(4,await db.Lines.CountAsync());}
    }
}
