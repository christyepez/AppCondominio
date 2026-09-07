using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AppCondominio.Contracts.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AppCondominio.IntegrationTests;

public sealed class PermissionAuthorizationTests
{
    private const string Issuer = "portal-corporativo";
    private const string Audience = "portal-corporativo-clients";
    private const string Secret = "TestOnly_S02_03_JwtSecret_AtLeast_32_Bytes!";

    [Fact]
    public async Task Organizations_endpoint_requires_authentication_and_permission()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var organizationId = Guid.NewGuid();

        var anonymous = await client.GetAsync($"/api/organizations/{organizationId}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateToken());

        var authenticatedWithoutPermission =
            await client.GetAsync($"/api/organizations/{organizationId}");

        Assert.Equal(HttpStatusCode.Forbidden, authenticatedWithoutPermission.StatusCode);

        var session = await client.GetAsync("/api/session");
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
    }

    [Fact]
    public async Task Read_permission_policy_succeeds_only_with_signed_permission_claim_shape()
    {
        await using var factory = CreateFactory();
        using var scope = factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();

        var allowedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(AppCondominioPermissions.ClaimType, AppCondominioPermissions.Organizations.Read)
        ], authenticationType: "test"));

        var deniedPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString())
        ], authenticationType: "test"));

        var allowed = await authorization.AuthorizeAsync(
            allowedPrincipal,
            resource: null,
            AppCondominioPermissions.Organizations.Read);

        var denied = await authorization.AuthorizeAsync(
            deniedPrincipal,
            resource: null,
            AppCondominioPermissions.Organizations.Read);

        Assert.True(allowed.Succeeded);
        Assert.False(denied.Succeeded);
    }

    private static WebApplicationFactory<Program> CreateFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:Secret"] = Secret,
                    ["ConnectionStrings:Organizations"] =
                        "Server=localhost;Database=AppCondominio;Integrated Security=True;TrustServerCertificate=True;"
                });
            }));

    private static string CreateToken()
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", Guid.NewGuid().ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
