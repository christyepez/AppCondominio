using AppCondominio.Contracts.Security;
using AppCondominio.Modules.ResidentPortal.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AppCondominio.Modules.ResidentPortal.Api;

public static class ResidentPortalEndpoints
{
    public static IServiceCollection AddResidentPortal(this IServiceCollection services)
    {
        services.AddScoped<ResidentPortalService>();
        return services;
    }

    public static IEndpointRouteBuilder MapResidentPortalEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/resident")
            .WithTags("Resident Portal")
            .RequireAuthorization(AppCondominioPermissions.ResidentPortal.Access);

        group.MapGet("/me", async (ICurrentIdentity identity, ResidentPortalService service, CancellationToken ct) =>
            Results.Ok(await service.GetContextAsync(RequireUser(identity), ct)));

        group.MapGet("/units/{unitId:guid}/statement", async (Guid unitId, ICurrentIdentity identity, ResidentPortalService service, CancellationToken ct) =>
            Results.Ok(await service.GetStatementAsync(RequireUser(identity), unitId, ct)));

        group.MapPost("/units/{unitId:guid}/reservations", async (Guid unitId, ReservationRequest r, ICurrentIdentity identity, ResidentPortalService service, CancellationToken ct) =>
            Results.Created("/api/resident/reservations", new { id = await service.RequestReservationAsync(RequireUser(identity), unitId, r.AreaId, r.StartsAt, r.EndsAt, r.Guests, ct) }));

        group.MapPost("/units/{unitId:guid}/visits", async (Guid unitId, VisitRequest r, ICurrentIdentity identity, ResidentPortalService service, CancellationToken ct) =>
            Results.Created("/api/resident/visits", new { id = await service.AuthorizeVisitAsync(RequireUser(identity), unitId, r.VisitorName, r.Document, r.Destination, r.ValidFrom, r.ValidTo, ct) }));

        return endpoints;
    }

    private static string RequireUser(ICurrentIdentity identity) => identity.IsAuthenticated && !string.IsNullOrWhiteSpace(identity.UserId)
        ? identity.UserId
        : throw new UnauthorizedAccessException("Authenticated Portal user identifier is required.");

    private sealed record ReservationRequest(Guid AreaId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int Guests);
    private sealed record VisitRequest(string VisitorName, string Document, string Destination, DateTimeOffset ValidFrom, DateTimeOffset ValidTo);
}
