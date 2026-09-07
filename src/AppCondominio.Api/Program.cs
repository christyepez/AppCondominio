using AppCondominio.Bootstrapper;
using AppCondominio.Contracts;
using AppCondominio.Contracts.Messaging;
using AppCondominio.Infrastructure;
using AppCondominio.Modules.Organizations.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddAppCondominioCache(builder.Configuration);
builder.Services.AddAppCondominioMessaging(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new ServiceInfo(
    Service: "AppCondominio.Api",
    Version: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
    Status: "ok")));

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapOrganizationsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/diagnostics/messaging/ping", async (IIntegrationEventPublisher publisher, CancellationToken cancellationToken) =>
    {
        var message = new FoundationPing(Guid.NewGuid(), DateTimeOffset.UtcNow, "AppCondominio.Api");
        await publisher.PublishAsync(message, cancellationToken);
        return Results.Accepted(value: message);
    });
}

app.Run();

public partial class Program;
