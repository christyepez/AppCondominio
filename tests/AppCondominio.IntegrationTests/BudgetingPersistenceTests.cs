using AppCondominio.Modules.Budgeting.Application;
using AppCondominio.Modules.Budgeting.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class BudgetingPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync();public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint13_runs_budget_approval_actuals_variance_and_summary()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Budgeting",_sql.GetConnectionString()}}).Build();var services=new ServiceCollection();services.AddBudgetingModule(config);await using var provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<BudgetingDbContext>();await db.Database.MigrateAsync();}
        var communityId=Guid.NewGuid();Guid planId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var s=scope.ServiceProvider.GetRequiredService<BudgetingService>();planId=await s.CreatePlanAsync(communityId,2026,1,"Operating budget",default);await s.AddLineAsync(planId,"5101","Maintenance",9,1000m,default);await s.AddLineAsync(planId,"5201","Utilities",9,500m,default);await s.ApproveAsync(planId,"admin",default);
            await s.ImportActualAsync(communityId,2026,9,"5101",900m,"accounting","journal-001",default);await s.ImportActualAsync(communityId,2026,9,"5201",650m,"accounting","journal-002",default);
            var variance=await s.GetVarianceAsync(planId,default);Assert.Equal(2,variance.Count);Assert.Equal(100m,variance.Single(x=>x.AccountCode=="5101").Variance);Assert.Equal(-150m,variance.Single(x=>x.AccountCode=="5201").Variance);
            var summary=await s.GetSummaryAsync(planId,9,default);Assert.Equal(1500m,summary.PlannedYtd);Assert.Equal(1550m,summary.ActualYtd);Assert.Equal(-50m,summary.VarianceYtd);Assert.Equal(1,summary.OverBudgetLines);Assert.Equal(1,summary.UnderBudgetLines);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>s.AddLineAsync(planId,"5301","Insurance",10,200m,default));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>s.ImportActualAsync(communityId,2026,9,"5101",10m,"accounting","journal-001",default));
        }
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<BudgetingDbContext>();Assert.Equal(1,await db.Plans.CountAsync());Assert.Equal(2,await db.Lines.CountAsync());Assert.Equal(2,await db.Actuals.CountAsync());}
    }
}
