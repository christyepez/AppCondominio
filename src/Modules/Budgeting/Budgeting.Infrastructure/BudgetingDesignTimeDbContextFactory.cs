using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppCondominio.Modules.Budgeting.Infrastructure;

internal sealed class BudgetingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<BudgetingDbContext>
{
    public BudgetingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Budgeting")
            ?? "Server=localhost,1433;Database=AppCondominioDesign;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<BudgetingDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "budgeting"))
            .Options;

        return new BudgetingDbContext(options);
    }
}

