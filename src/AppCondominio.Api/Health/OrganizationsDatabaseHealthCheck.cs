using AppCondominio.Modules.Organizations.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class OrganizationsDatabaseHealthCheck(
    OrganizationsDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Organizations SQL Server is reachable.")
                : HealthCheckResult.Unhealthy("Organizations SQL Server is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Organizations SQL Server readiness check failed.",
                exception);
        }
    }
}
