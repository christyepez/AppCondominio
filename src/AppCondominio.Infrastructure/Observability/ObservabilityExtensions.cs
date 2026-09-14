using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Infrastructure.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddAppCondominioObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        bool includeAspNetCoreInstrumentation)
    {
        var seqUrl = configuration["Seq:Url"] ?? "http://seq";

        services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .WriteTo.Console()
            .WriteTo.Seq(seqUrl));

        var tracing = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(builder =>
            {
                builder.AddHttpClientInstrumentation();
                if (includeAspNetCoreInstrumentation)
                {
                    builder.AddAspNetCoreInstrumentation();
                }

                var endpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    builder.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
                }
            });

        return services;
    }
}
