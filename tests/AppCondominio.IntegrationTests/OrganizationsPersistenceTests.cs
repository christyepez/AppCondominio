using AppCondominio.Modules.Organizations.Domain;
using AppCondominio.Modules.Organizations.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class OrganizationsPersistenceTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder().Build();

    public Task InitializeAsync() => _sql.StartAsync();

    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Organization_can_be_persisted_and_loaded()
    {
        var options = new DbContextOptionsBuilder<OrganizationsDbContext>()
            .UseSqlServer(_sql.GetConnectionString(), sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "organizations"))
            .Options;

        await using var db = new OrganizationsDbContext(options);
        await db.Database.MigrateAsync();

        var organization = Organization.Create("Condominio Piloto", "1790012345001");
        db.Organizations.Add(organization);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var loaded = await db.Organizations.SingleAsync(x => x.Id == organization.Id);
        Assert.Equal("Condominio Piloto", loaded.Name);
    }
}
