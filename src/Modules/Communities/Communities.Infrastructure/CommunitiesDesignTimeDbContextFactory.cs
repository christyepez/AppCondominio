using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Communities.Infrastructure;

internal sealed class CommunitiesDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CommunitiesDbContext>
{
    public CommunitiesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Communities")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<CommunitiesDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "communities"))
            .Options;

        return new CommunitiesDbContext(options);
    }
}

