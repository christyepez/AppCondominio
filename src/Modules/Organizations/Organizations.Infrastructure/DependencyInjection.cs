using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Application.CreateOrganization;
using AppCondominio.Modules.Organizations.Application.GetOrganizationById;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.Organizations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrganizationsModule(this IServiceCollection services)
    {
        services.AddSingleton<IOrganizationRepository, InMemoryOrganizationRepository>();
        services.AddScoped<CreateOrganizationHandler>();
        services.AddScoped<GetOrganizationByIdHandler>();
        return services;
    }
}
