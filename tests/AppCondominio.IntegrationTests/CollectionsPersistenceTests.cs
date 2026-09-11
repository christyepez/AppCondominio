using AppCondominio.Contracts.Messaging;
using AppCondominio.Modules.Collections.Application;
using AppCondominio.Modules.Collections.Domain;
using AppCondominio.Modules.Collections.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class CollectionsPersistenceTests:IAsyncLifetime
{
    private readonly MsSqlContainer _sql=new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync()=>_sql.StartAsync(); public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint09_registers_applies_and_reverses_payment_on_sql_server()
    {
        IConfiguration config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:Collections",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();services.AddSingleton<IIntegrationEventPublisher,TestPublisher>();services.AddCollectionsModule(config);await using ServiceProvider provider=services.BuildServiceProvider();
        await using(var scope=provider.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<CollectionsDbContext>();await db.Database.MigrateAsync();}

        Guid communityId=Guid.NewGuid();Guid unitId=Guid.NewGuid();Guid responsible=Guid.NewGuid();Guid receivableId;Guid paymentId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<CollectionsService>();
            receivableId=await service.CreateReceivableAsync(communityId,unitId,responsible,Guid.NewGuid(),100m,new DateOnly(2026,9,1),new DateOnly(2026,9,10),default);
            paymentId=await service.RegisterPaymentAsync(communityId,"DEP-001",PaymentMethod.Deposit,new DateOnly(2026,9,8),60m,"BANK-001",default);
            _=await service.ApplyPaymentAsync(paymentId,receivableId,60m,"collector",default);
            var payment=await service.GetPaymentAsync(paymentId,default);Assert.Equal(0m,payment.UnappliedAmount);Assert.Equal(PaymentStatus.Applied,payment.Status);
            var rows=await service.GetReceivablesAsync(communityId,unitId,new DateOnly(2026,9,11),default);Assert.Single(rows);Assert.Equal(40m,rows[0].OutstandingAmount);
            await service.ReversePaymentAsync(paymentId,"supervisor",default);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<CollectionsDbContext>();
            Assert.Equal(PaymentStatus.Reversed,(await db.Payments.SingleAsync(x=>x.Id==paymentId)).Status);
            Assert.Equal(100m,(await db.Receivables.SingleAsync(x=>x.Id==receivableId)).OutstandingAmount);
            Assert.True((await db.Applications.SingleAsync()).IsReversed);
        }
    }

    private sealed class TestPublisher:IIntegrationEventPublisher
    {
        public Task PublishAsync<T>(T message,CancellationToken cancellationToken=default) where T:class=>Task.CompletedTask;
    }
}
