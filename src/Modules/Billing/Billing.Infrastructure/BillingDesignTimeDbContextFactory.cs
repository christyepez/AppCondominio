using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Billing.Infrastructure;

internal sealed class BillingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Billing")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing"))
            .Options;

        return new BillingDbContext(options);
    }
}

