using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.People.Infrastructure;

internal sealed class PeopleDesignTimeDbContextFactory : IDesignTimeDbContextFactory<PeopleDbContext>
{
    public PeopleDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__People")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<PeopleDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "people"))
            .Options;

        return new PeopleDbContext(options);
    }
}

