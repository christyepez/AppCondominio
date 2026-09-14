using AppCondominio.Modules.Tax.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class TaxDatabaseHealthCheck(TaxDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Tax SQL Server is reachable."):HealthCheckResult.Unhealthy("Tax SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("Tax SQL Server readiness check failed.",ex);}
    }
}
