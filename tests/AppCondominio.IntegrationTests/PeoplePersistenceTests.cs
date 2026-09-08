using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Domain;
using AppCondominio.Modules.People.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class PeoplePersistenceTests:IAsyncLifetime
{
    private const string SqlServerImage="mcr.microsoft.com/mssql/server:2022-latest";
    private readonly MsSqlContainer _sql=new MsSqlBuilder(SqlServerImage).Build();
    public Task InitializeAsync()=>_sql.StartAsync();
    public Task DisposeAsync()=>_sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint06_persists_people_ownership_and_activation_lifecycle()
    {
        IConfiguration configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:People",_sql.GetConnectionString()}}).Build();
        var services=new ServiceCollection();
        services.AddPeopleModule(configuration);
        services.AddResidentAccessContext();
        await using ServiceProvider provider=services.BuildServiceProvider();

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<PeopleDbContext>();
            await db.Database.MigrateAsync();
        }

        Guid communityId=Guid.NewGuid();
        Guid unitId=Guid.NewGuid();
        Guid grantId;
        Guid tenantId;
        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<PeopleService>();
            Guid ownerId=await service.CreateNaturalPersonAsync(communityId,"OWNER-DEMO","Owner Demo",null,null,null,null,default);
            tenantId=await service.CreateNaturalPersonAsync(communityId,"TENANT-DEMO","Tenant Demo",null,null,null,null,default);
            await service.AddOwnershipAsync(communityId,unitId,ownerId,100m,new DateOnly(2026,1,1),"purchase",null,default);
            await service.AddResidentAsync(communityId,unitId,tenantId,OccupancyRole.Tenant,new DateOnly(2026,9,1),default);
            await service.CreateLeaseAsync(communityId,unitId,tenantId,ownerId,new DateOnly(2026,9,1),new DateOnly(2026,9,30),new[]{"view","payments"},null,default);
            await service.CreateAccessCodeAsync(communityId,unitId,"activation-code",DateTimeOffset.UtcNow.AddMinutes(30),default);
            Guid activationId=await service.RequestActivationAsync(communityId,unitId,tenantId,"portal-demo-user",OccupancyRole.Tenant,"activation-code",default);
            grantId=await service.ApproveActivationAsync(activationId,"admin-demo",DateTimeOffset.UtcNow.AddMinutes(10),default);
            await service.SetFinancialResponsibilityAsync(communityId,unitId,ownerId,new DateOnly(2026,1,1),default);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<PeopleDbContext>();
            Assert.Equal(2,await db.People.CountAsync(x=>x.CommunityId==communityId));
            Assert.Equal(100m,await db.Ownerships.Where(x=>x.CommunityId==communityId&&x.UnitId==unitId&&x.EndsOn==null).SumAsync(x=>x.Percentage));
            Assert.True(await db.AccessCodes.AllAsync(x=>x.CodeHash!="activation-code"));
            Assert.Equal(ActivationStatus.Approved,(await db.Activations.SingleAsync()).Status);
            Assert.Equal(AccessGrantStatus.Active,(await db.AccessGrants.SingleAsync(x=>x.Id==grantId)).Status);

            var residentAccess=scope.ServiceProvider.GetRequiredService<ResidentAccessService>();
            ResidentUnitAccess access=Assert.Single(await residentAccess.GetCurrentAsync("portal-demo-user",default));
            Assert.Equal(communityId,access.CommunityId);
            Assert.Equal(unitId,access.UnitId);
            Assert.Equal(tenantId,access.PersonId);
            Assert.Equal("Tenant Demo",access.DisplayName);
        }

        await using(var scope=provider.CreateAsyncScope())
        {
            var service=scope.ServiceProvider.GetRequiredService<PeopleService>();
            AccessExpiryResult result=await service.ExpireTenantAccessAsync(communityId,DateTimeOffset.UtcNow.AddDays(40),default);
            Assert.Equal(1,result.LeasesExpired);
            Assert.Equal(1,result.GrantsExpired);
            Assert.Contains("portal-demo-user",result.ExternalUsersPendingPortalRevocation);
        }
    }
}
