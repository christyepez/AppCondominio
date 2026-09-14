using AppCondominio.Modules.Collections.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class CollectionsDatabaseHealthCheck(CollectionsDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Collections SQL Server is reachable."):HealthCheckResult.Unhealthy("Collections SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("Collections SQL Server readiness check failed.",ex);}
    }
}
