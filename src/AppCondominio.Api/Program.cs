using AppCondominio.Api;
using AppCondominio.Api.Security;
using AppCondominio.Bootstrapper;
using AppCondominio.Contracts;
using AppCondominio.Contracts.Messaging;
using AppCondominio.Contracts.Security;
using AppCondominio.Infrastructure;
using AppCondominio.Infrastructure.Observability;
using AppCondominio.Modules.Organizations.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddAppCondominioCache(builder.Configuration);
builder.Services.AddAppCondominioMessaging(builder.Configuration);
builder.Services.AddAppCondominioObservability(builder.Configuration, "AppCondominio.Api", includeAspNetCoreInstrumentation: true);
builder.Services.AddPortalCompatibleJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentIdentity, HttpCurrentIdentity>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new ServiceInfo(
    Service: "AppCondominio.Api",
    Version: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
    Status: "ok")));

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapGet("/api/session", (ICurrentIdentity identity) => Results.Ok(new
{
    identity.IsAuthenticated,
    identity.UserId,
    Permissions = identity.Permissions.OrderBy(x => x)
})).RequireAuthorization();
app.MapOrganizationsEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/diagnostics/messaging/ping", async (IIntegrationEventPublisher publisher, CancellationToken cancellationToken) =>
    {
        var message = new FoundationPing(Guid.NewGuid(), DateTimeOffset.UtcNow, "AppCondominio.Api");
        await publisher.PublishAsync(message, cancellationToken);
        return Results.Accepted(value: message);
    }).RequireAuthorization();
}

app.Run();

public partial class Program;
