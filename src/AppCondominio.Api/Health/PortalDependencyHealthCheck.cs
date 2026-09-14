using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class PortalDependencyHealthCheck(
    IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClientFactory
                .CreateClient("PortalGatewayHealth")
                .GetAsync("health/ready", cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("PortalCorporativo gateway readiness endpoint is reachable.")
                : HealthCheckResult.Unhealthy(
                    $"PortalCorporativo gateway readiness returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "PortalCorporativo gateway readiness check failed.",
                exception);
        }
    }
}
