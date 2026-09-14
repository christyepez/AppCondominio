using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Properties.Infrastructure;

internal sealed class PropertiesDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PropertiesDbContext>
{
    public PropertiesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Properties")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<PropertiesDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "properties"))
            .Options;

        return new PropertiesDbContext(options);
    }
}

