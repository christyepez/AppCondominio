using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class RabbitMqHealthCheck(
    IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["RabbitMq:Host"] ?? "rabbitmq";
        var port = configuration.GetValue<int?>("RabbitMq:Port") ?? 5672;

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeout.Token);

            return client.Connected
                ? HealthCheckResult.Healthy("RabbitMQ endpoint is reachable.")
                : HealthCheckResult.Unhealthy("RabbitMQ endpoint is not reachable.");
        }
        catch (Exception exception) when (exception is SocketException or OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ readiness check failed.", exception);
        }
    }
}
