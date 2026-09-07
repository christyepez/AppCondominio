using AppCondominio.Contracts.Messaging;
using MassTransit;

namespace AppCondominio.Infrastructure.Messaging;

internal sealed class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class => publishEndpoint.Publish(message, cancellationToken);
}
