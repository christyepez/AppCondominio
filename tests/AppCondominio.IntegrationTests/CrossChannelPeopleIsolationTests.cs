using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Infrastructure;
using AppCondominio.Modules.Reservations.Application;
using AppCondominio.Modules.Reservations.Infrastructure;
using AppCondominio.Modules.SecurityOperations.Application;
using AppCondominio.Modules.SecurityOperations.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class CrossChannelPeopleIsolationTests : IAsyncLifetime
{
    private readonly MsSqlContainer sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public Task InitializeAsync() => sql.StartAsync();
    public Task DisposeAsync() => sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Reservations_and_security_reject_people_from_another_community()
    {
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
            ["ConnectionStrings:People"] = sql.GetConnectionString(), ["ConnectionStrings:Reservations"] = sql.GetConnectionString(),
            ["ConnectionStrings:SecurityOperations"] = sql.GetConnectionString() }).Build();
        var services = new ServiceCollection(); services.AddPeopleModule(config); services.AddReservationsModule(config); services.AddSecurityOperationsModule(config);
        await using var provider = services.BuildServiceProvider();
        await using (var scope = provider.CreateAsyncScope()) {
            await scope.ServiceProvider.GetRequiredService<PeopleDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<ReservationsDbContext>().Database.MigrateAsync();
            await scope.ServiceProvider.GetRequiredService<SecurityOperationsDbContext>().Database.MigrateAsync();
        }
        var communityA = Guid.NewGuid(); var communityB = Guid.NewGuid();
        await using var testScope = provider.CreateAsyncScope();
        var people = testScope.ServiceProvider.GetRequiredService<PeopleService>();
        var reservations = testScope.ServiceProvider.GetRequiredService<ReservationService>();
        var security = testScope.ServiceProvider.GetRequiredService<SecurityOperationsService>();
        var personA = await people.CreateNaturalPersonAsync(communityA,"ID-A","Person A",null,null,null,null,default);
        var personB = await people.CreateNaturalPersonAsync(communityB,"ID-B","Person B",null,null,null,null,default);
        var areaA = await reservations.CreateAreaAsync(communityA,"SOC","Social Area",20,0,default);
        var starts = DateTimeOffset.UtcNow.AddDays(1); var ends = starts.AddHours(2);
        var booking = await reservations.RequestAsync(areaA,personA,starts,ends,2,default);
        Assert.NotEqual(Guid.Empty, booking);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reservations.RequestAsync(areaA,personB,starts.AddDays(1),ends.AddDays(1),2,default));
        var visit = await security.AuthorizeVisitAsync(communityA,personA,"Visitor A","DOC-A","Tower A",starts,ends,default);
        Assert.NotEqual(Guid.Empty, visit);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => security.AuthorizeVisitAsync(communityA,personB,"Visitor B","DOC-B","Tower A",starts,ends,default));
    }
}
