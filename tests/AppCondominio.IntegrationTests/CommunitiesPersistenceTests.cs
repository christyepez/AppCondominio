using AppCondominio.Modules.Communities.Domain;
using AppCondominio.Modules.Communities.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace AppCondominio.IntegrationTests;

public sealed class CommunitiesPersistenceTests : IAsyncLifetime
{
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private readonly MsSqlContainer _sql = new MsSqlBuilder(SqlServerImage).Build();

    public Task InitializeAsync() => _sql.StartAsync();

    public Task DisposeAsync() => _sql.DisposeAsync().AsTask();

    [Fact]
    public async Task Sprint04_schema_persists_community_configuration()
    {
        var options = new DbContextOptionsBuilder<CommunitiesDbContext>()
            .UseSqlServer(_sql.GetConnectionString(), sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "communities"))
            .Options;

        await using var db = new CommunitiesDbContext(options);
        await db.Database.MigrateAsync();

        var organizationId = Guid.NewGuid();
        var community = Community.Create(organizationId, "demo-01", "Conjunto Demo", "1790012345001", "Quito");
        community.Update("Conjunto Demo", "1790012345001", "Quito", "Condominio", "Administrador Demo", "Presidente Demo");
        var tax = TaxSettings.Create(community.Id, "Conjunto Demo", "1790012345001");
        tax.Update("Conjunto Demo", "1790012345001", "001", "001", false, "TEST");
        var node = StructureNode.Create(community.Id, null, StructureNodeType.Stage, "E1", "Etapa 1");
        var area = CommonArea.Create(community.Id, "SALON", "Salón Comunal", true);
        var bank = CommunityBankAccount.Create(community.Id, "Banco Demo", "123456", BankAccountType.Checking, "USD", "Collections", "1.1.01");
        var charge = ChargeSettings.Create(community.Id);
        charge.Update(1, 10, 0.02m, "interest,oldest,current,credit", "USD", 2026);
        var authority = CommunityAuthority.Create(community.Id, "President", "Presidente Demo", new DateOnly(2026, 1, 1), "content://authority/1");
        var document = CommunityDocument.Register(community.Id, "regulation", "Reglamento", "content://community/regulation/1");

        db.AddRange(community, tax, node, area, bank, charge, authority, document);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.Equal(1, await db.Communities.CountAsync());
        Assert.Equal(1, await db.TaxSettings.CountAsync());
        Assert.Equal(1, await db.StructureNodes.CountAsync());
        Assert.Equal(1, await db.CommonAreas.CountAsync());
        Assert.Equal(1, await db.BankAccounts.CountAsync());
        Assert.Equal(1, await db.ChargeSettings.CountAsync());
        Assert.Equal(1, await db.Authorities.CountAsync());
        Assert.Equal(1, await db.Documents.CountAsync());
    }
}
