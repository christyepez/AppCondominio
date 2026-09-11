using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Budgeting.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Budgeting.Api;

public static class BudgetingEndpoints
{
    public static IEndpointRouteBuilder MapBudgetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group=app.MapGroup("/api/budgeting").RequireAuthorization();
        group.MapGet("/communities/{communityId:guid}/plans",async(Guid communityId,BudgetingService s,CancellationToken ct)=>Results.Ok(await s.ListPlansAsync(communityId,ct))).RequireAuthorization(AppCondominioPermissions.Budgeting.Read);
        group.MapPost("/plans",async(CreateBudgetPlanRequest r,BudgetingService s,CancellationToken ct)=>Results.Ok(new{Id=await s.CreatePlanAsync(r.CommunityId,r.Year,r.Version,r.Name,ct)})).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapPost("/plans/{planId:guid}/lines",async(Guid planId,CreateBudgetLineRequest r,BudgetingService s,CancellationToken ct)=>Results.Ok(new{Id=await s.AddLineAsync(planId,r.AccountCode,r.Category,r.Month,r.PlannedAmount,ct)})).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapPut("/lines/{lineId:guid}/amount",async(Guid lineId,ChangeBudgetLineAmountRequest r,BudgetingService s,CancellationToken ct)=>{await s.ChangeLineAmountAsync(lineId,r.Amount,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapPost("/plans/{planId:guid}/approve",async(Guid planId,DecisionRequest r,BudgetingService s,CancellationToken ct)=>{await s.ApproveAsync(planId,r.By,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapPost("/plans/{planId:guid}/close",async(Guid planId,DecisionRequest r,BudgetingService s,CancellationToken ct)=>{await s.CloseAsync(planId,r.By,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapPost("/actuals",async(ImportBudgetActualRequest r,BudgetingService s,CancellationToken ct)=>Results.Ok(new{Id=await s.ImportActualAsync(r.CommunityId,r.Year,r.Month,r.AccountCode,r.Amount,r.SourceType,r.SourceReference,ct)})).RequireAuthorization(AppCondominioPermissions.Budgeting.Manage);
        group.MapGet("/plans/{planId:guid}/variance",async(Guid planId,BudgetingService s,CancellationToken ct)=>Results.Ok(await s.GetVarianceAsync(planId,ct))).RequireAuthorization(AppCondominioPermissions.Budgeting.Read);
        group.MapGet("/plans/{planId:guid}/summary",async(Guid planId,int throughMonth,BudgetingService s,CancellationToken ct)=>Results.Ok(await s.GetSummaryAsync(planId,throughMonth,ct))).RequireAuthorization(AppCondominioPermissions.Budgeting.Read);
        return app;
    }
}

public sealed record CreateBudgetPlanRequest(Guid CommunityId,int Year,int Version,string Name);
public sealed record CreateBudgetLineRequest(string AccountCode,string Category,int Month,decimal PlannedAmount);
public sealed record ChangeBudgetLineAmountRequest(decimal Amount);
public sealed record DecisionRequest(string By);
public sealed record ImportBudgetActualRequest(Guid CommunityId,int Year,int Month,string AccountCode,decimal Amount,string SourceType,string SourceReference);
