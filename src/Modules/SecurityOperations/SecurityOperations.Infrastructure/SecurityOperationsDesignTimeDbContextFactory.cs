using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.SecurityOperations.Infrastructure;

internal sealed class SecurityOperationsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SecurityOperationsDbContext>
{
    public SecurityOperationsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SecurityOperations")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<SecurityOperationsDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "securityops"))
            .Options;

        return new SecurityOperationsDbContext(options);
    }
}

