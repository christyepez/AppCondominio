using AppCondominio.Bootstrapper;
using AppCondominio.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
