using AppCondominio.Modules.Accounting.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace AppCondominio.Api.Health;
public sealed class AccountingDatabaseHealthCheck(AccountingDbContext dbContext):IHealthCheck{public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default){try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Accounting SQL Server is reachable."):HealthCheckResult.Unhealthy("Accounting SQL Server is not reachable.");}catch(Exception ex){return HealthCheckResult.Unhealthy("Accounting SQL Server readiness check failed.",ex);}}}
