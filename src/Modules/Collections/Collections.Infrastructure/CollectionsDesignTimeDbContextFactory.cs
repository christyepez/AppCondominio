using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Collections.Infrastructure;

internal sealed class CollectionsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CollectionsDbContext>
{
    public CollectionsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Collections")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<CollectionsDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "collections"))
            .Options;

        return new CollectionsDbContext(options);
    }
}

