using AppCondominio.Contracts.Messaging;
using AppCondominio.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAppCondominioCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("Redis connection is required. Configure ConnectionStrings:Redis or Redis:ConnectionString.");
        }

        services.AddStackExchangeRedisCache(options => options.Configuration = connection);
        return services;
    }

    public static IServiceCollection AddAppCondominioMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var host = configuration["RabbitMq:Host"] ?? "rabbitmq";
        var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
        var username = configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("RabbitMq:Username is required.");
        var password = configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("RabbitMq:Password is required.");

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        return services;
    }
}
