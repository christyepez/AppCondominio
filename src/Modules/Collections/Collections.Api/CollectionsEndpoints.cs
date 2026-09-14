using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Collections.Application;
using AppCondominio.Modules.Collections.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Collections.Api;

public static class CollectionsEndpoints
{
    public static IEndpointRouteBuilder MapCollectionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/collections").WithTags("Collections").RequireAuthorization();
        g.MapPost("/receivables",async(CreateReceivableRequest r,CollectionsService s,CancellationToken ct)=>Results.Created("/api/collections/receivables",new{id=await s.CreateReceivableAsync(r.CommunityId,r.UnitId,r.ResponsiblePersonId,r.BillingObligationId,r.Amount,r.IssuedOn,r.DueOn,ct)})).RequireAuthorization(AppCondominioPermissions.Collections.Manage);
        g.MapPost("/payments",async(RegisterPaymentRequest r,CollectionsService s,CancellationToken ct)=>Results.Created("/api/collections/payments",new{id=await s.RegisterPaymentAsync(r.CommunityId,r.Reference,r.Method,r.ReceivedOn,r.Amount,r.ExternalTransactionId,ct)})).RequireAuthorization(AppCondominioPermissions.Collections.Manage);
        g.MapPost("/payments/{paymentId:guid}/applications",async(Guid paymentId,ApplyPaymentRequest r,CollectionsService s,CancellationToken ct)=>Results.Created($"/api/collections/payments/{paymentId}/applications",new{id=await s.ApplyPaymentAsync(paymentId,r.ReceivableId,r.Amount,r.AppliedBy,ct)})).RequireAuthorization(AppCondominioPermissions.Collections.Manage);
        g.MapPost("/payments/{paymentId:guid}/reverse",async(Guid paymentId,ReversePaymentRequest r,CollectionsService s,CancellationToken ct)=>{await s.ReversePaymentAsync(paymentId,r.ReversedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Collections.Manage);
        g.MapGet("/payments/{paymentId:guid}",async(Guid paymentId,CollectionsService s,CancellationToken ct)=>Results.Ok(await s.GetPaymentAsync(paymentId,ct))).RequireAuthorization(AppCondominioPermissions.Collections.Read);
        g.MapGet("/communities/{communityId:guid}/receivables",async(Guid communityId,Guid? unitId,DateOnly? on,CollectionsService s,CancellationToken ct)=>Results.Ok(await s.GetReceivablesAsync(communityId,unitId,on??DateOnly.FromDateTime(DateTime.UtcNow),ct))).RequireAuthorization(AppCondominioPermissions.Collections.Read);
        g.MapGet("/communities/{communityId:guid}/aging",async(Guid communityId,DateOnly? on,CollectionsService s,CancellationToken ct)=>Results.Ok(await s.GetAgingAsync(communityId,on??DateOnly.FromDateTime(DateTime.UtcNow),ct))).RequireAuthorization(AppCondominioPermissions.Collections.Read);
        return endpoints;
    }

    private sealed record CreateReceivableRequest(Guid CommunityId,Guid UnitId,Guid ResponsiblePersonId,Guid BillingObligationId,decimal Amount,DateOnly IssuedOn,DateOnly DueOn);
    private sealed record RegisterPaymentRequest(Guid CommunityId,string Reference,PaymentMethod Method,DateOnly ReceivedOn,decimal Amount,string? ExternalTransactionId);
    private sealed record ApplyPaymentRequest(Guid ReceivableId,decimal Amount,string AppliedBy);
    private sealed record ReversePaymentRequest(string ReversedBy);
}
