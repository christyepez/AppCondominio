using AppCondominio.Bootstrapper;
using AppCondominio.Contracts;
using AppCondominio.Modules.Organizations.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppCondominio(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new ServiceInfo(
    Service: "AppCondominio.Api",
    Version: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
    Status: "ok")));

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapOrganizationsEndpoints();

app.Run();

public partial class Program;
