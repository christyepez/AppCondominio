using AppCondominio.Modules.Procurement.Application;
using AppCondominio.Modules.Procurement.Infrastructure;
using AppCondominio.Modules.Treasury.Application;
using AppCondominio.Modules.Treasury.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class TreasuryProcurementIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync() => _sql.StartAsync();
    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Treasury_lists_only_eligible_suppliers_and_rejects_cross_community_supplier()
    {
        IConfiguration cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Procurement",_sql.GetConnectionString()},{"ConnectionStrings:Treasury",_sql.GetConnectionString()}}).Build();
        var services = new ServiceCollection();services.AddProcurementModule(cfg);services.AddTreasuryModule(cfg);await using var provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){await scope.ServiceProvider.GetRequiredService<ProcurementDbContext>().Database.MigrateAsync();await scope.ServiceProvider.GetRequiredService<TreasuryDbContext>().Database.MigrateAsync();}
        var communityA=Guid.NewGuid();var communityB=Guid.NewGuid();Guid supplierA,supplierB;
        await using(var scope=provider.CreateAsyncScope()){var procurement=scope.ServiceProvider.GetRequiredService<ProcurementService>();supplierA=await procurement.CreateSupplierAsync(communityA,null,"RUC-A","Supplier A","a@example.com",default);supplierB=await procurement.CreateSupplierAsync(communityB,null,"RUC-B","Supplier B","b@example.com",default);}
        await using(var scope=provider.CreateAsyncScope()){var treasury=scope.ServiceProvider.GetRequiredService<TreasuryService>();var eligible=await treasury.ListEligibleSuppliersAsync(communityA,default);Assert.Single(eligible);Assert.Equal(supplierA,eligible[0].Id);var payable=await treasury.CreatePayableAsync(communityA,supplierA,"INV-A",new DateOnly(2026,9,9),new DateOnly(2026,9,30),100m,"5101","2101",default);Assert.NotEqual(Guid.Empty,payable);await Assert.ThrowsAsync<InvalidOperationException>(()=>treasury.CreatePayableAsync(communityA,supplierB,"INV-B",new DateOnly(2026,9,9),new DateOnly(2026,9,30),100m,"5101","2101",default));}
    }
}
