using AppCondominio.Modules.People.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class PeopleDatabaseHealthCheck(PeopleDbContext dbContext):IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default)
    {
        try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("People SQL Server is reachable."):HealthCheckResult.Unhealthy("People SQL Server is not reachable.");}
        catch(Exception ex){return HealthCheckResult.Unhealthy("People SQL Server readiness check failed.",ex);}
    }
}
