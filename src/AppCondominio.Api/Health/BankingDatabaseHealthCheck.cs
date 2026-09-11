using AppCondominio.Modules.Banking.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class BankingDatabaseHealthCheck(BankingDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Banking SQL Server is reachable."):HealthCheckResult.Unhealthy("Banking SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("Banking SQL Server readiness check failed.",ex);}
    }
}
