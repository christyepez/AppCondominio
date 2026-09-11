using AppCondominio.Modules.Billing.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppCondominio.Worker;

public sealed class MonthlyBillingSchedulerWorker(IServiceScopeFactory scopeFactory,IConfiguration configuration,ILogger<MonthlyBillingSchedulerWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!configuration.GetValue("BillingScheduler:Enabled",false))
        {
            logger.LogInformation("Monthly billing scheduler is disabled.");
            return;
        }

        int intervalMinutes=Math.Clamp(configuration.GetValue("BillingScheduler:IntervalMinutes",60),5,1440);
        using var timer=new PeriodicTimer(TimeSpan.FromMinutes(intervalMinutes));
        await RunOnce(stoppingToken);
        while(await timer.WaitForNextTickAsync(stoppingToken))await RunOnce(stoppingToken);
    }

    private async Task RunOnce(CancellationToken ct)
    {
        await using var scope=scopeFactory.CreateAsyncScope();
        var service=scope.ServiceProvider.GetRequiredService<MonthlyBillingService>();
        int issued=await service.IssueApprovedDuePeriodsAsync(DateOnly.FromDateTime(DateTime.UtcNow),ct);
        logger.LogInformation("Monthly billing scheduler issued {IssuedLines} charge obligations.",issued);
    }
}
