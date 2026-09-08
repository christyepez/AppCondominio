using AppCondominio.Api;
using AppCondominio.Api.Health;
using AppCondominio.Api.Security;
using AppCondominio.Bootstrapper;
using AppCondominio.Contracts;
using AppCondominio.Contracts.Messaging;
using AppCondominio.Contracts.Security;
using AppCondominio.Infrastructure;
using AppCondominio.Infrastructure.Observability;
using AppCondominio.Modules.Communities.Api;
using AppCondominio.Modules.Organizations.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddAppCondominioCache(builder.Configuration);
builder.Services.AddAppCondominioMessaging(builder.Configuration);
builder.Services.AddAppCondominioObservability(builder.Configuration,"AppCondominio.Api",includeAspNetCoreInstrumentation:true);
builder.Services.AddPortalCompatibleJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<ICurrentIdentity,HttpCurrentIdentity>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();builder.Services.AddProblemDetails();
var portalGatewayBaseUrl=builder.Configuration["Portal:GatewayBaseUrl"];
if(string.IsNullOrWhiteSpace(portalGatewayBaseUrl))throw new InvalidOperationException("Portal:GatewayBaseUrl is required.");
builder.Services.AddHttpClient("PortalGatewayHealth",c=>{c.BaseAddress=new Uri(portalGatewayBaseUrl,UriKind.Absolute);c.Timeout=TimeSpan.FromSeconds(3);});
builder.Services.AddHealthChecks()
    .AddCheck<OrganizationsDatabaseHealthCheck>("organizations-sql",tags:["ready"])
    .AddCheck<CommunitiesDatabaseHealthCheck>("communities-sql",tags:["ready"])
    .AddCheck<RedisHealthCheck>("redis",tags:["ready"])
    .AddCheck<RabbitMqHealthCheck>("rabbitmq",tags:["ready"])
    .AddCheck<PortalDependencyHealthCheck>("portal-gateway",tags:["ready","portal"]);
var app=builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();app.UseExceptionHandler();app.UseSerilogRequestLogging();app.UseAuthentication();app.UseAuthorization();
app.MapGet("/",()=>Results.Ok(new ServiceInfo("AppCondominio.Api",typeof(Program).Assembly.GetName().Version?.ToString()??"0.0.0","ok")));
app.MapHealthChecks("/health/live",new HealthCheckOptions{Predicate=_=>false});
app.MapHealthChecks("/health/ready",new HealthCheckOptions{Predicate=r=>r.Tags.Contains("ready")});
app.MapGet("/api/session",(ICurrentIdentity identity)=>Results.Ok(new{identity.IsAuthenticated,identity.UserId,Permissions=identity.Permissions.OrderBy(x=>x)})).RequireAuthorization();
app.MapOrganizationsEndpoints();app.MapSaasEndpoints();app.MapCommunityEndpoints();
if(app.Environment.IsDevelopment())app.MapPost("/diagnostics/messaging/ping",async(IIntegrationEventPublisher publisher,CancellationToken ct)=>{var message=new FoundationPing(Guid.NewGuid(),DateTimeOffset.UtcNow,"AppCondominio.Api");await publisher.PublishAsync(message,ct);return Results.Accepted(value:message);}).RequireAuthorization();
app.Run();
public partial class Program;
