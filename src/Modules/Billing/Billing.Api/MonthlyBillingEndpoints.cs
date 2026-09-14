using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Billing.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Billing.Api;

public static class MonthlyBillingEndpoints
{
    public static IEndpointRouteBuilder MapMonthlyBillingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/billing/monthly").WithTags("Billing Monthly").RequireAuthorization();
        g.MapPost("/periods",async(OpenPeriodRequest r,MonthlyBillingService s,CancellationToken ct)=>Results.Created("/api/billing/monthly/periods",new{id=await s.OpenPeriodAsync(r.CommunityId,r.Year,r.Month,r.IssueDate,r.DueDate,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/periods/{periodId:guid}/draft",async(Guid periodId,DraftRequest r,MonthlyBillingService s,CancellationToken ct)=>Results.Ok(await s.GenerateDraftAsync(periodId,r.Candidates.Select(x=>new DraftCandidateInput(x.UnitId,x.ResponsiblePersonId,x.VersionId,new ChargeSimulationInput(x.Area,x.TotalArea,x.Coefficient,x.Consumption,x.PercentageBase,x.ManualAmount,x.ProrationFactor))).ToArray(),ct))).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/periods/{periodId:guid}/approve",async(Guid periodId,ApprovePeriodRequest r,MonthlyBillingService s,CancellationToken ct)=>{await s.ApproveAsync(periodId,r.ApprovedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/periods/{periodId:guid}/issue",async(Guid periodId,MonthlyBillingService s,CancellationToken ct)=>Results.Ok(new{issued=await s.IssueAsync(periodId,ct)})).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapPost("/periods/{periodId:guid}/reverse",async(Guid periodId,ReversePeriodRequest r,MonthlyBillingService s,CancellationToken ct)=>{await s.ReverseAsync(periodId,r.Reason,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        g.MapGet("/communities/{communityId:guid}/units/{unitId:guid}/statement",async(Guid communityId,Guid unitId,MonthlyBillingService s,CancellationToken ct)=>Results.Ok(await s.GetStatementAsync(communityId,unitId,ct))).RequireAuthorization(AppCondominioPermissions.Billing.Read);
        g.MapPost("/communities/{communityId:guid}/units/{unitId:guid}/statement/notify",async(Guid communityId,Guid unitId,NotifyStatementRequest r,MonthlyBillingService s,CancellationToken ct)=>{await s.NotifyStatementAsync(communityId,unitId,r.ResponsiblePersonId,r.Year,r.Month,ct);return Results.Accepted();}).RequireAuthorization(AppCondominioPermissions.Billing.Manage);
        return endpoints;
    }

    private sealed record OpenPeriodRequest(Guid CommunityId,int Year,int Month,DateOnly IssueDate,DateOnly DueDate);
    private sealed record DraftRequest(IReadOnlyList<DraftCandidateRequest> Candidates);
    private sealed record DraftCandidateRequest(Guid UnitId,Guid ResponsiblePersonId,Guid VersionId,decimal Area,decimal TotalArea,decimal Coefficient,decimal Consumption,decimal PercentageBase,decimal ManualAmount,decimal ProrationFactor=1m);
    private sealed record ApprovePeriodRequest(string ApprovedBy);
    private sealed record ReversePeriodRequest(string Reason);
    private sealed record NotifyStatementRequest(Guid ResponsiblePersonId,int Year,int Month);
}
