using AppCondominio.Bootstrapper;
using AppCondominio.Infrastructure;
using AppCondominio.Infrastructure.Observability;
using AppCondominio.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddAppCondominioCache(builder.Configuration);
builder.Services.AddAppCondominioMessaging(builder.Configuration, x => x.AddConsumer<FoundationPingConsumer>());
builder.Services.AddAppCondominioObservability(builder.Configuration, "AppCondominio.Worker", includeAspNetCoreInstrumentation: false);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
