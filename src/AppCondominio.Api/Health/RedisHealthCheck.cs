using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppCondominio.Api.Health;

public sealed class RedisHealthCheck(
    IDistributedCache cache) : IHealthCheck
{
    private const string ProbeKey = "appcondominio:health:redis";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var value = Guid.NewGuid().ToString("N");
            await cache.SetStringAsync(
                ProbeKey,
                value,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                },
                cancellationToken);

            var roundTrip = await cache.GetStringAsync(ProbeKey, cancellationToken);
            await cache.RemoveAsync(ProbeKey, cancellationToken);

            return string.Equals(value, roundTrip, StringComparison.Ordinal)
                ? HealthCheckResult.Healthy("Redis read/write round-trip succeeded.")
                : HealthCheckResult.Unhealthy("Redis read/write round-trip returned an unexpected value.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis readiness check failed.", exception);
        }
    }
}
