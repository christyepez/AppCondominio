using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Properties.Application;
using AppCondominio.Modules.Properties.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Properties.Api;

public static class PropertyEndpoints
{
    public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/properties").WithTags("Properties").RequireAuthorization();

        group.MapPost("/types", async (CreateTypeRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created("/api/properties/types", new { id = await service.CreateTypeAsync(new(request.CommunityId, request.Code, request.Name), ct) }))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        group.MapPost("/units", async (CreateUnitRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created("/api/properties/units", new { id = await service.CreateUnitAsync(new(request.CommunityId, request.TypeId, request.Code, request.Location, request.MainAreaM2, request.Notes), ct) }))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        group.MapPost("/units/{unitId:guid}/areas", async (Guid unitId, AreaRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created($"/api/properties/units/{unitId}/areas", new { id = await service.AddAreaAsync(new(request.CommunityId, unitId, request.Type, request.AreaM2, request.IsComputable, request.Description), ct) }))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        group.MapPost("/relations", async (RelationRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created("/api/properties/relations", new { id = await service.LinkUnitAsync(new(request.CommunityId, request.MainUnitId, request.RelatedUnitId, request.RelationType, request.StartsOn), ct) }))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        group.MapPost("/aliquots", async (AliquotRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created("/api/properties/aliquots", new { id = await service.CreateAliquotAsync(new(request.CommunityId, request.UnitId, request.Method, request.UnitComputableArea, request.TotalComputableArea, request.Coefficient, request.FixedPercentage, request.ManualValue, request.ValidFrom, request.Reason, request.SupportDocumentReference, request.ApprovedBy), ct) }))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        group.MapGet("/communities/{communityId:guid}/units", async (Guid communityId, PropertyService service, CancellationToken ct) =>
            Results.Ok(await service.ListUnitsAsync(communityId, ct)))
            .RequireAuthorization(AppCondominioPermissions.Properties.Read);

        group.MapGet("/communities/{communityId:guid}/units/{unitId:guid}", async (Guid communityId, Guid unitId, PropertyService service, CancellationToken ct) =>
        {
            PropertyRecordResult? result = await service.GetRecordAsync(communityId, unitId, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization(AppCondominioPermissions.Properties.Read);

        group.MapGet("/communities/{communityId:guid}/coefficient-validation", async (Guid communityId, PropertyService service, CancellationToken ct) =>
            Results.Ok(await service.ValidateCoefficientsAsync(communityId, ct)))
            .RequireAuthorization(AppCondominioPermissions.Properties.Read);

        group.MapPost("/communities/{communityId:guid}/demo", async (Guid communityId, DemoRequest request, PropertyService service, CancellationToken ct) =>
            Results.Created($"/api/properties/communities/{communityId}/demo", await service.GenerateDemoAsync(communityId, request.UnitCount, ct)))
            .RequireAuthorization(AppCondominioPermissions.Properties.Manage);

        return endpoints;
    }

    private sealed record CreateTypeRequest(Guid CommunityId, string Code, string Name);
    private sealed record CreateUnitRequest(Guid CommunityId, Guid TypeId, string Code, string Location, decimal MainAreaM2, string? Notes);
    private sealed record AreaRequest(Guid CommunityId, PropertyAreaType Type, decimal AreaM2, bool IsComputable, string? Description);
    private sealed record RelationRequest(Guid CommunityId, Guid MainUnitId, Guid RelatedUnitId, PropertyRelationType RelationType, DateOnly StartsOn);
    private sealed record AliquotRequest(Guid CommunityId, Guid UnitId, AliquotCalculationMethod Method, decimal UnitComputableArea, decimal TotalComputableArea, decimal Coefficient, decimal FixedPercentage, decimal ManualValue, DateOnly ValidFrom, string Reason, string? SupportDocumentReference, string ApprovedBy);
    private sealed record DemoRequest(int UnitCount = 300);
}
