using AppCondominio.Modules.Budgeting.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace AppCondominio.Api.Health;
public sealed class BudgetingDatabaseHealthCheck(BudgetingDbContext dbContext):IHealthCheck{public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,CancellationToken cancellationToken=default){try{return await dbContext.Database.CanConnectAsync(cancellationToken)?HealthCheckResult.Healthy("Budgeting SQL Server is reachable."):HealthCheckResult.Unhealthy("Budgeting SQL Server is not reachable.");}catch(Exception ex){return HealthCheckResult.Unhealthy("Budgeting SQL Server readiness check failed.",ex);}}}
