using AppCondominio.Modules.Governance.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class GovernanceDatabaseHealthCheck(GovernanceDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Governance SQL Server is reachable.")
                : HealthCheckResult.Unhealthy("Governance SQL Server is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Governance SQL Server readiness check failed.", exception);
        }
    }
}
