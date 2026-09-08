using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Procurement.Application;
using Microsoft.AspNetCore.Builder;using Microsoft.AspNetCore.Http;using Microsoft.AspNetCore.Routing;
namespace AppCondominio.Modules.Procurement.Api;
public static class ProcurementEndpoints
{
 public static IEndpointRouteBuilder MapProcurementEndpoints(this IEndpointRouteBuilder app)
 {
  var g=app.MapGroup("/api/procurement").RequireAuthorization();
  g.MapPost("/suppliers",async(CreateSupplierRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.CreateSupplierAsync(r.CommunityId,r.PersonId,r.TaxId,r.LegalName,r.Email,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/suppliers/{supplierId:guid}/portal-access",async(Guid supplierId,GrantSupplierAccessRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.GrantSupplierPortalAccessAsync(supplierId,r.ExternalUserId,r.ExpiresAtUtc,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/supplier-access/{accessId:guid}/revoke",async(Guid accessId,RevokeSupplierAccessRequest r,ProcurementService s,CancellationToken ct)=>{await s.RevokeSupplierPortalAccessAsync(accessId,r.Reason,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/requisitions",async(CreateRequisitionRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.CreateRequisitionAsync(r.CommunityId,r.Number,r.Description,r.EstimatedAmount,r.RequestedBy,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/requisitions/{id:guid}/approve",async(Guid id,DecisionRequest r,ProcurementService s,CancellationToken ct)=>{await s.ApproveRequisitionAsync(id,r.By,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/requisitions/{id:guid}/rounds",async(Guid id,OpenRoundRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.OpenRoundAsync(id,r.Title,r.ClosesAtUtc,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/rounds/{roundId:guid}/bids",async(Guid roundId,SubmitBidRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.SubmitBidAsync(roundId,r.SupplierId,r.Amount,r.DeliveryDays,r.ProposalReference,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/bids/{bidId:guid}/score",async(Guid bidId,ScoreBidRequest r,ProcurementService s,CancellationToken ct)=>{await s.ScoreBidAsync(bidId,r.Score,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapPost("/rounds/{roundId:guid}/close",async(Guid roundId,ProcurementService s,CancellationToken ct)=>{await s.CloseRoundAsync(roundId,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);
  g.MapGet("/rounds/{roundId:guid}/ranking",async(Guid roundId,ProcurementService s,CancellationToken ct)=>Results.Ok(await s.GetRankingAsync(roundId,ct))).RequireAuthorization(AppCondominioPermissions.Procurement.Read);
  g.MapPost("/rounds/{roundId:guid}/award",async(Guid roundId,AwardRequest r,ProcurementService s,CancellationToken ct)=>Results.Ok(new{Id=await s.AwardAsync(roundId,r.BidId,r.OrderNumber,r.PayableReference,ct)})).RequireAuthorization(AppCondominioPermissions.Procurement.Manage);

  var supplier=app.MapGroup("/api/supplier").WithTags("Supplier Portal").RequireAuthorization(AppCondominioPermissions.SupplierPortal.Access);
  supplier.MapGet("/me",async(ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>Results.Ok(await s.GetSupplierPortalContextAsync(RequireUser(identity),ct)));
  supplier.MapGet("/rounds",async(ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>Results.Ok(await s.GetSupplierOpenRoundsAsync(RequireUser(identity),ct)));
  supplier.MapGet("/bids",async(ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>Results.Ok(await s.GetOwnBidsAsync(RequireUser(identity),ct)));
  supplier.MapGet("/orders",async(ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>Results.Ok(await s.GetOwnOrdersAsync(RequireUser(identity),ct)));
  supplier.MapPost("/rounds/{roundId:guid}/bids",async(Guid roundId,SupplierPortalBidRequest r,ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>Results.Created("/api/supplier/bids",new{Id=await s.SubmitOwnBidAsync(RequireUser(identity),roundId,r.Amount,r.DeliveryDays,r.ProposalReference,ct)}));
  supplier.MapPost("/orders/{orderId:guid}/acknowledge",async(Guid orderId,ICurrentIdentity identity,ProcurementService s,CancellationToken ct)=>{await s.AcknowledgeOwnOrderAsync(RequireUser(identity),orderId,ct);return Results.NoContent();});
  return app;
 }
 private static string RequireUser(ICurrentIdentity identity)=>identity.IsAuthenticated&&!string.IsNullOrWhiteSpace(identity.UserId)?identity.UserId:throw new UnauthorizedAccessException("Authenticated Portal user identifier is required.");
}
public sealed record CreateSupplierRequest(Guid CommunityId,Guid? PersonId,string TaxId,string LegalName,string Email);public sealed record GrantSupplierAccessRequest(string ExternalUserId,DateTimeOffset? ExpiresAtUtc);public sealed record RevokeSupplierAccessRequest(string Reason);public sealed record CreateRequisitionRequest(Guid CommunityId,string Number,string Description,decimal EstimatedAmount,string RequestedBy);public sealed record DecisionRequest(string By);public sealed record OpenRoundRequest(string Title,DateTimeOffset ClosesAtUtc);public sealed record SubmitBidRequest(Guid SupplierId,decimal Amount,int DeliveryDays,string ProposalReference);public sealed record SupplierPortalBidRequest(decimal Amount,int DeliveryDays,string ProposalReference);public sealed record ScoreBidRequest(decimal Score);public sealed record AwardRequest(Guid BidId,string OrderNumber,string PayableReference);