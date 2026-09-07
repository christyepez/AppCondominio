using AppCondominio.Modules.Organizations.Application.CreateOrganization;
using AppCondominio.Modules.Organizations.Application.GetOrganizationById;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Organizations.Api;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/organizations")
            .WithTags("Organizations")
            .RequireAuthorization();

        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:guid}", GetByIdAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateOrganizationRequest request,
        CreateOrganizationHandler handler,
        CancellationToken cancellationToken)
    {
        CreateOrganizationResult result = await handler.HandleAsync(
            new CreateOrganizationCommand(request.Name, request.TaxId),
            cancellationToken);

        return Results.Created($"/api/organizations/{result.Id}", result);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetOrganizationByIdHandler handler,
        CancellationToken cancellationToken)
    {
        GetOrganizationByIdResult? result = await handler.HandleAsync(
            new GetOrganizationByIdQuery(id),
            cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private sealed record CreateOrganizationRequest(string Name, string? TaxId);
}
