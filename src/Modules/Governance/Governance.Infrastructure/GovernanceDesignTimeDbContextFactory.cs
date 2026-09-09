using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Governance.Infrastructure;

internal sealed class GovernanceDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GovernanceDbContext>
{
    public GovernanceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Governance")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<GovernanceDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "governance"))
            .Options;

        return new GovernanceDbContext(options);
    }
}

