using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Accounting.Application;
using AppCondominio.Modules.Accounting.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Accounting.Api;

public static class AccountingEndpoints
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/accounting").WithTags("Accounting").RequireAuthorization();
        g.MapPost("/accounts",async(CreateAccountRequest r,AccountingService s,CancellationToken ct)=>Results.Created("/api/accounting/accounts",new{id=await s.CreateAccountAsync(r.CommunityId,r.Code,r.Name,r.Type,r.AllowsPosting,ct)})).RequireAuthorization(AppCondominioPermissions.Accounting.Manage);
        g.MapPost("/periods",async(OpenPeriodRequest r,AccountingService s,CancellationToken ct)=>Results.Created("/api/accounting/periods",new{id=await s.OpenPeriodAsync(r.CommunityId,r.Year,r.Month,ct)})).RequireAuthorization(AppCondominioPermissions.Accounting.Manage);
        g.MapPost("/periods/{id:guid}/close",async(Guid id,ClosePeriodRequest r,AccountingService s,CancellationToken ct)=>{await s.ClosePeriodAsync(id,r.ClosedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Accounting.Manage);
        g.MapPost("/journals",async(PostJournalRequest r,AccountingService s,CancellationToken ct)=>Results.Created("/api/accounting/journals",new{id=await s.PostAsync(r.CommunityId,r.PeriodId,r.EntryDate,r.Description,r.SourceType,r.SourceReference,r.Lines.Select(x=>new JournalLineInput(x.AccountId,x.Debit,x.Credit,x.CostCenter)).ToArray(),r.PostedBy,ct)})).RequireAuthorization(AppCondominioPermissions.Accounting.Manage);
        g.MapPost("/journals/{id:guid}/reverse",async(Guid id,ReverseJournalRequest r,AccountingService s,CancellationToken ct)=>{await s.ReverseAsync(id,r.ReversedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Accounting.Manage);
        g.MapGet("/communities/{communityId:guid}/trial-balance",async(Guid communityId,AccountingService s,CancellationToken ct)=>Results.Ok(await s.GetTrialBalanceAsync(communityId,ct))).RequireAuthorization(AppCondominioPermissions.Accounting.Read);
        g.MapGet("/communities/{communityId:guid}/accounts/{accountId:guid}/ledger",async(Guid communityId,Guid accountId,AccountingService s,CancellationToken ct)=>Results.Ok(await s.GetLedgerAsync(communityId,accountId,ct))).RequireAuthorization(AppCondominioPermissions.Accounting.Read);
        return endpoints;
    }
    private sealed record CreateAccountRequest(Guid CommunityId,string Code,string Name,AccountType Type,bool AllowsPosting=true);
    private sealed record OpenPeriodRequest(Guid CommunityId,int Year,int Month);
    private sealed record ClosePeriodRequest(string ClosedBy);
    private sealed record PostJournalRequest(Guid CommunityId,Guid PeriodId,DateOnly EntryDate,string Description,string SourceType,string SourceReference,IReadOnlyList<PostJournalLineRequest> Lines,string PostedBy);
    private sealed record PostJournalLineRequest(Guid AccountId,decimal Debit,decimal Credit,string? CostCenter);
    private sealed record ReverseJournalRequest(string ReversedBy);
}
