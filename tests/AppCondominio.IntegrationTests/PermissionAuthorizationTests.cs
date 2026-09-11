using System.Security.Claims;
using AppCondominio.Api.Security;
using AppCondominio.Contracts.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.IntegrationTests;

public sealed class PermissionAuthorizationTests
{
    private const string Issuer = "portal-corporativo";
    private const string Audience = "portal-corporativo-clients";
    private const string Secret = "TestOnly_S02_03_JwtSecret_AtLeast_32_Bytes!";

    [Fact]
    public async Task Read_policy_denies_anonymous_and_authenticated_user_without_permission()
    {
        using var provider = CreateProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var authenticatedWithoutPermission = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString())
        ], authenticationType: "test"));

        var anonymousResult = await authorization.AuthorizeAsync(
            anonymous,
            resource: null,
            AppCondominioPermissions.Organizations.Read);

        var authenticatedResult = await authorization.AuthorizeAsync(
            authenticatedWithoutPermission,
            resource: null,
            AppCondominioPermissions.Organizations.Read);

        Assert.False(anonymousResult.Succeeded);
        Assert.False(authenticatedResult.Succeeded);
    }

    [Fact]
    public async Task Read_and_manage_policies_require_the_matching_permission_claim()
    {
        using var provider = CreateProvider();
        var authorization = provider.GetRequiredService<IAuthorizationService>();

        var readPrincipal = PrincipalWith(AppCondominioPermissions.Organizations.Read);
        var managePrincipal = PrincipalWith(AppCondominioPermissions.Organizations.Manage);

        var readAllowed = await authorization.AuthorizeAsync(
            readPrincipal,
            resource: null,
            AppCondominioPermissions.Organizations.Read);
        var readCannotManage = await authorization.AuthorizeAsync(
            readPrincipal,
            resource: null,
            AppCondominioPermissions.Organizations.Manage);
        var manageAllowed = await authorization.AuthorizeAsync(
            managePrincipal,
            resource: null,
            AppCondominioPermissions.Organizations.Manage);

        Assert.True(readAllowed.Succeeded);
        Assert.False(readCannotManage.Succeeded);
        Assert.True(manageAllowed.Succeeded);
    }

    private static ServiceProvider CreateProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:Secret"] = Secret
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPortalCompatibleJwtAuthentication(configuration);
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal PrincipalWith(string permission) =>
        new(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(AppCondominioPermissions.ClaimType, permission)
        ], authenticationType: "test"));
}
