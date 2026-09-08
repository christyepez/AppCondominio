using AppCondominio.Modules.Billing.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class BillingDatabaseHealthCheck(BillingDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Billing SQL Server is reachable."):HealthCheckResult.Unhealthy("Billing SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("Billing SQL Server readiness check failed.",ex);}
    }
}
