using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Application.CreateOrganization;
using AppCondominio.Modules.Organizations.Application.GetOrganizationById;
using AppCondominio.Modules.Organizations.Application.Saas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Organizations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Organizations")
            ?? throw new InvalidOperationException(
                "Connection string 'Organizations' is required. Configure it through ConnectionStrings__Organizations.");

        services.AddDbContext<OrganizationsDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "organizations")));

        services.AddScoped<IOrganizationRepository, EfOrganizationRepository>();
        services.AddScoped<ISaasRepository, EfSaasRepository>();
        services.AddScoped<ISaasMetricsRepository, EfSaasMetricsRepository>();
        services.AddScoped<CreateOrganizationHandler>();
        services.AddScoped<GetOrganizationByIdHandler>();
        services.AddScoped<SaasService>();
        services.AddScoped<SaasAdministrationService>();

        return services;
    }
}
