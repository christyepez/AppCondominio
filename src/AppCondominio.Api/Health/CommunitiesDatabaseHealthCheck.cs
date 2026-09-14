using AppCondominio.Modules.Communities.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class CommunitiesDatabaseHealthCheck(CommunitiesDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Communities SQL Server is reachable."):HealthCheckResult.Unhealthy("Communities SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("Communities SQL Server readiness check failed.",ex);}
    }
}
