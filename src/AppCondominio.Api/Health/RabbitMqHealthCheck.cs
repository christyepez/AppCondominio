using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class RabbitMqHealthCheck(
    IBusHealth busHealth) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var status = busHealth.CheckHealth();

        return Task.FromResult(status.Status switch
        {
            BusHealthStatus.Healthy => HealthCheckResult.Healthy("RabbitMQ bus is healthy."),
            BusHealthStatus.Degraded => HealthCheckResult.Degraded("RabbitMQ bus is degraded."),
            _ => HealthCheckResult.Unhealthy("RabbitMQ bus is not healthy.")
        });
    }
}
