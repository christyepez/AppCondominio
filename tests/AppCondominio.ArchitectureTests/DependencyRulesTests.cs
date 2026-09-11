using System.Reflection;
using AppCondominio.Modules.Organizations.Application.Abstractions;
using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.ArchitectureTests;

public sealed class DependencyRulesTests
{
    [Fact]
    public void Organizations_domain_does_not_reference_infrastructure_or_aspnetcore()
    {
        var references = typeof(Organization).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain(references, x => x is not null && x.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(references, x => x is not null && x.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }

    [Fact]
    public void Organizations_application_does_not_reference_infrastructure()
    {
        Assembly assembly = typeof(IOrganizationRepository).Assembly;
        var references = assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain(references, x => x is not null && x.Contains("Organizations.Infrastructure", StringComparison.OrdinalIgnoreCase));
    }
}
