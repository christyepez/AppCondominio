using AppCondominio.Modules.Tax.Application;
using AppCondominio.Modules.Tax.Domain;
using AppCondominio.Modules.Tax.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class TaxPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync(); public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint10_persists_document_and_fails_closed_when_sri_is_not_configured()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Tax",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();services.AddTaxModule(config);await using var provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<TaxDbContext>();await db.Database.MigrateAsync();}

        Guid id;
        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<TaxService>();
            id=await service.CreateAsync(Guid.NewGuid(),Guid.NewGuid(),"invoice","001","001","000000001","TEST-ACCESS-KEY-0001","content://invoice/1",default);
            var result=await service.SubmitAsync(id,default);
            Assert.Equal(ElectronicDocumentStatus.PendingConfiguration,result.Status);
            Assert.Null(result.AuthorizationNumber);
            Assert.Contains("not configured",result.LastMessage,StringComparison.OrdinalIgnoreCase);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<TaxDbContext>();
            var stored=await db.Documents.SingleAsync(x=>x.Id==id);
            Assert.Equal(ElectronicDocumentStatus.PendingConfiguration,stored.Status);
            Assert.Null(stored.AuthorizationNumber);
        }
    }
}
