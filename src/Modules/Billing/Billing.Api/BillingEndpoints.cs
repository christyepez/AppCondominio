using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Billing.Api;

public static class BillingEndpoints
{
    public static IEndpointRouteBuilder MapBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/billing").WithTags("Billing").RequireAuthorization();
        g.MapPost("/concepts",async(ConceptRequest r,BillingService s,CancellationToken ct)=>Results.Created("/api/billing/concepts",new{id=await s.CreateConceptAsync(r.CommunityId,r.Code,r.Name,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/versions",async(VersionRequest r,BillingService s,CancellationToken ct)=>Results.Created("/api/billing/versions",new{id=await s.CreateVersionAsync(r.CommunityId,r.ConceptId,r.Method,r.Periodicity,r.ValidFrom,r.FixedAmount,r.Rate,r.Minimum,r.Maximum,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/versions/{id:guid}/approve",async(Guid id,ApproveRequest r,BillingService s,CancellationToken ct)=>{await s.ApproveVersionAsync(id,r.ApprovedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/accounting",async(AccountingRequest r,BillingService s,CancellationToken ct)=>Results.Created("/api/billing/accounting",new{id=await s.MapAccountingAsync(r.CommunityId,r.ConceptId,r.RevenueAccount,r.ReceivableAccount,r.TaxAccount,r.CostCenter,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/interest",async(InterestRequest r,BillingService s,CancellationToken ct)=>Results.Created("/api/billing/interest",new{id=await s.AddInterestAsync(r.CommunityId,r.ConceptId,r.Percentage,r.FixedAmount,r.GraceDays,r.EveryDays,r.MaximumAmount,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/discounts",async(DiscountRequest r,BillingService s,CancellationToken ct)=>Results.Created("/api/billing/discounts",new{id=await s.AddDiscountAsync(r.CommunityId,r.ConceptId,r.ValidFrom,r.ValidTo,r.Percentage,r.FixedAmount,r.PayBeforeDueDays,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/versions/{versionId:guid}/simulate",async(Guid versionId,SimulationRequest r,BillingService s,CancellationToken ct)=>Results.Ok(await s.SimulateAsync(versionId,new(r.Area,r.TotalArea,r.Coefficient,r.Consumption,r.PercentageBase,r.ManualAmount,r.ProrationFactor),r.ChargeDate,r.DueDate,r.DaysLate,ct))).RequireAuthorization(AppCondominioPermissions.Billing.Read);
        return endpoints;
    }
    private sealed record ConceptRequest(Guid CommunityId,string Code,string Name);
    private sealed record VersionRequest(Guid CommunityId,Guid ConceptId,ChargeCalculationMethod Method,ChargePeriodicity Periodicity,DateOnly ValidFrom,decimal FixedAmount,decimal Rate,decimal Minimum,decimal Maximum);
    private sealed record ApproveRequest(string ApprovedBy);
    private sealed record AccountingRequest(Guid CommunityId,Guid ConceptId,string RevenueAccount,string ReceivableAccount,string? TaxAccount,string? CostCenter);
    private sealed record InterestRequest(Guid CommunityId,Guid ConceptId,decimal Percentage,decimal FixedAmount,int GraceDays,int EveryDays,decimal MaximumAmount);
    private sealed record DiscountRequest(Guid CommunityId,Guid ConceptId,DateOnly ValidFrom,DateOnly? ValidTo,decimal Percentage,decimal FixedAmount,int PayBeforeDueDays);
    private sealed record SimulationRequest(decimal Area,decimal TotalArea,decimal Coefficient,decimal Consumption,decimal PercentageBase,decimal ManualAmount,decimal ProrationFactor,DateOnly ChargeDate,DateOnly DueDate,int DaysLate);
}
