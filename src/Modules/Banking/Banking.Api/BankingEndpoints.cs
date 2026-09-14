using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Banking.Application;
using AppCondominio.Modules.Banking.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Banking.Api;

public static class BankingEndpoints
{
    public static IEndpointRouteBuilder MapBankingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/banking").WithTags("Banking").RequireAuthorization();
        g.MapPost("/accounts",async(CreateAccountRequest r,BankingService s,CancellationToken ct)=>Results.Created("/api/banking/accounts",new{id=await s.CreateAccountAsync(r.CommunityId,r.BankCode,r.AccountNumber,r.Currency,ct)})).RequireAuthorization(AppCondominioPermissions.Banking.Manage);
        g.MapPost("/accounts/{accountId:guid}/transactions/import",async(Guid accountId,ImportTransactionsRequest r,BankingService s,CancellationToken ct)=>Results.Ok(new{imported=await s.ImportAsync(r.CommunityId,accountId,r.Rows.Select(x=>new ImportedBankTransaction(x.ImportKey,x.BookingDate,x.Amount,x.Type,x.Reference,x.Description)).ToArray(),ct)})).RequireAuthorization(AppCondominioPermissions.Banking.Manage);
        g.MapPost("/communities/{communityId:guid}/reconciliation/suggest",async(Guid communityId,SuggestRequest r,BankingService s,CancellationToken ct)=>Results.Ok(await s.SuggestAsync(communityId,r.Payments.Select(x=>new PaymentCandidate(x.PaymentId,x.ReceivedOn,x.Amount,x.Reference,x.ExternalTransactionId)).ToArray(),r.MinimumConfidence,ct))).RequireAuthorization(AppCondominioPermissions.Banking.Manage);
        g.MapPost("/transactions/{transactionId:guid}/match",async(Guid transactionId,MatchRequest r,BankingService s,CancellationToken ct)=>{await s.MatchAsync(transactionId,r.PaymentId,r.DecidedBy,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Banking.Manage);
        return endpoints;
    }
    private sealed record CreateAccountRequest(Guid CommunityId,string BankCode,string AccountNumber,string Currency="USD");
    private sealed record ImportTransactionsRequest(Guid CommunityId,IReadOnlyList<ImportRowRequest> Rows);
    private sealed record ImportRowRequest(string ImportKey,DateOnly BookingDate,decimal Amount,BankTransactionType Type,string Reference,string Description);
    private sealed record SuggestRequest(decimal MinimumConfidence,IReadOnlyList<PaymentCandidateRequest> Payments);
    private sealed record PaymentCandidateRequest(Guid PaymentId,DateOnly ReceivedOn,decimal Amount,string Reference,string? ExternalTransactionId);
    private sealed record MatchRequest(Guid PaymentId,string DecidedBy);
}
