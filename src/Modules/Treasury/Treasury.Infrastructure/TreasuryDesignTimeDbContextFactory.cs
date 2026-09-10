using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Treasury.Infrastructure;

internal sealed class TreasuryDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TreasuryDbContext>
{
    public TreasuryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Treasury")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<TreasuryDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "treasury"))
            .Options;

        return new TreasuryDbContext(options);
    }
}

