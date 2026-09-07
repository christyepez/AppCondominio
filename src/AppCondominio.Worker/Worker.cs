using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppCondominio.Worker;

public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("AppCondominio Worker started at {StartedAtUtc}", DateTime.UtcNow);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
