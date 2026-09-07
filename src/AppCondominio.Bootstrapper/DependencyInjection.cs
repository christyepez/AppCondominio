using AppCondominio.Modules.Organizations.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Bootstrapper;

public static class DependencyInjection
{
    public static IServiceCollection AddAppCondominio(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOrganizationsModule();
        return services;
    }
}
