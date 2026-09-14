using AppCondominio.Modules.Properties.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class PropertiesDatabaseHealthCheck(PropertiesDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Properties SQL Server is reachable.")
                : HealthCheckResult.Unhealthy("Properties SQL Server is not reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Properties SQL Server readiness check failed.", ex);
        }
    }
}
