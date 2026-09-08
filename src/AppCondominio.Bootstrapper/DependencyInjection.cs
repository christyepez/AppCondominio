using AppCondominio.Modules.Banking.Infrastructure;
using AppCondominio.Modules.Billing.Infrastructure;
using AppCondominio.Modules.Collections.Infrastructure;
using AppCondominio.Modules.Communities.Infrastructure;
using AppCondominio.Modules.Organizations.Infrastructure;
using AppCondominio.Modules.People.Infrastructure;
using AppCondominio.Modules.Properties.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Bootstrapper;

public static class DependencyInjection
{
    public static IServiceCollection AddAppCondominio(this IServiceCollection services,IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);ArgumentNullException.ThrowIfNull(configuration);
        services.AddOrganizationsModule(configuration);
        services.AddCommunitiesModule(configuration);
        services.AddPropertiesModule(configuration);
        services.AddPeopleModule(configuration);
        services.AddBillingModule(configuration);
        services.AddCollectionsModule(configuration);
        services.AddBankingModule(configuration);
        return services;
    }
}
