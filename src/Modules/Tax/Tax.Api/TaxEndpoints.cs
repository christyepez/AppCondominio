using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Tax.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Tax.Api;

public static class TaxEndpoints
{
    public static IEndpointRouteBuilder MapTaxEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/tax").WithTags("Tax").RequireAuthorization();
        g.MapGet("/communities/{communityId:guid}/documents",async(Guid communityId,TaxService s,CancellationToken ct)=>Results.Ok(await s.ListAsync(communityId,ct))).RequireAuthorization(AppCondominioPermissions.Tax.Read);
        g.MapPost("/documents",async(CreateTaxDocumentRequest r,TaxService s,CancellationToken ct)=>Results.Created("/api/tax/documents",new{id=await s.CreateAsync(r.CommunityId,r.SourceDocumentId,r.DocumentType,r.EstablishmentCode,r.EmissionPoint,r.Sequential,r.AccessKey,r.PayloadReference,ct)})).RequireAuthorization(AppCondominioPermissions.Tax.Manage);
        g.MapPost("/documents/{id:guid}/submit",async(Guid id,TaxService s,CancellationToken ct)=>Results.Ok(await s.SubmitAsync(id,ct))).RequireAuthorization(AppCondominioPermissions.Tax.Manage);
        g.MapPost("/documents/{id:guid}/refresh",async(Guid id,TaxService s,CancellationToken ct)=>Results.Ok(await s.RefreshAsync(id,ct))).RequireAuthorization(AppCondominioPermissions.Tax.Manage);
        g.MapGet("/documents/{id:guid}",async(Guid id,TaxService s,CancellationToken ct)=>Results.Ok(await s.GetAsync(id,ct))).RequireAuthorization(AppCondominioPermissions.Tax.Read);
        return endpoints;
    }

    private sealed record CreateTaxDocumentRequest(Guid CommunityId,Guid SourceDocumentId,string DocumentType,string EstablishmentCode,string EmissionPoint,string Sequential,string AccessKey,string PayloadReference);
}
