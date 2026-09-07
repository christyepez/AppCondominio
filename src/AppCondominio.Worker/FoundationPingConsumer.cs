using AppCondominio.Contracts.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AppCondominio.Worker;

public sealed class FoundationPingConsumer(ILogger<FoundationPingConsumer> logger) : IConsumer<FoundationPing>
{
    public Task Consume(ConsumeContext<FoundationPing> context)
    {
        logger.LogInformation(
            "Foundation ping consumed. MessageId={MessageId} Source={Source} CreatedAtUtc={CreatedAtUtc}",
            context.Message.MessageId,
            context.Message.Source,
            context.Message.CreatedAtUtc);
        return Task.CompletedTask;
    }
}
