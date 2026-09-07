using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class RabbitMqHealthCheck(
    IBus bus) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var status = bus.CheckHealth();

        return Task.FromResult(status.Status switch
        {
            BusHealthStatus.Healthy => HealthCheckResult.Healthy("RabbitMQ bus is healthy."),
            BusHealthStatus.Degraded => HealthCheckResult.Degraded("RabbitMQ bus is degraded."),
            _ => HealthCheckResult.Unhealthy("RabbitMQ bus is not healthy.")
        });
    }
}
