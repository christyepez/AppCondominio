using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Communities.Application;
using AppCondominio.Modules.Communities.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Communities.Api;

public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var g=endpoints.MapGroup("/api/communities").WithTags("Communities").RequireAuthorization();
        g.MapGet("/",async(CommunityService s,CancellationToken ct)=>Results.Ok(await s.ListAsync(ct))).RequireAuthorization(AppCondominioPermissions.Communities.Read);
        g.MapPost("/",async(CreateCommunityRequest r,CommunityService s,CancellationToken ct)=>Results.Created("/api/communities",await s.CreateAsync(new(r.OrganizationId,r.Code,r.Name,r.TaxId,r.Address),ct))).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapGet("/{id:guid}",async(Guid id,CommunityService s,CancellationToken ct)=>Results.Ok(await s.GetAsync(id,ct))).RequireAuthorization(AppCondominioPermissions.Communities.Read);
        g.MapPut("/{id:guid}",async(Guid id,UpdateCommunityRequest r,CommunityService s,CancellationToken ct)=>{await s.UpdateAsync(id,r.Name,r.TaxId,r.Address,r.Type,r.Administrator,r.President,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPut("/{id:guid}/tax",async(Guid id,TaxRequest r,CommunityService s,CancellationToken ct)=>{await s.AddTaxAsync(id,r.LegalName,r.TaxId,r.Establishment,r.EmissionPoint,r.AccountingObligation,r.SriEnvironment,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPost("/{id:guid}/structure",async(Guid id,NodeRequest r,CommunityService s,CancellationToken ct)=>Results.Created($"/api/communities/{id}/structure",new{id=await s.AddNodeAsync(id,r.ParentId,r.Type,r.Code,r.Name,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPost("/{id:guid}/common-areas",async(Guid id,AreaRequest r,CommunityService s,CancellationToken ct)=>Results.Created($"/api/communities/{id}/common-areas",new{id=await s.AddCommonAreaAsync(id,r.Code,r.Name,r.Reservable,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPost("/{id:guid}/bank-accounts",async(Guid id,BankRequest r,CommunityService s,CancellationToken ct)=>Results.Created($"/api/communities/{id}/bank-accounts",new{id=await s.AddBankAsync(id,r.Bank,r.Number,r.Type,r.Currency,r.Use,r.AccountingAccount,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPut("/{id:guid}/charge-settings",async(Guid id,ChargeRequest r,CommunityService s,CancellationToken ct)=>Results.Ok(new{id=await s.AddChargeSettingsAsync(id,r.IssueDay,r.DueDay,r.InterestRate,r.ApplicationOrder,r.Currency,r.FiscalYear,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPost("/{id:guid}/authorities",async(Guid id,AuthorityRequest r,CommunityService s,CancellationToken ct)=>Results.Created($"/api/communities/{id}/authorities",new{id=await s.AddAuthorityAsync(id,r.Role,r.PersonName,r.StartsOn,r.SupportDocumentReference,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapPost("/{id:guid}/documents",async(Guid id,DocumentRequest r,CommunityService s,CancellationToken ct)=>Results.Created($"/api/communities/{id}/documents",new{id=await s.AddDocumentAsync(id,r.DocumentType,r.Name,r.ExternalReference,ct)})).RequireAuthorization(AppCondominioPermissions.Communities.Manage);
        g.MapGet("/dashboard",async(CommunityService s,CancellationToken ct)=>Results.Ok(await s.DashboardAsync(ct))).RequireAuthorization(AppCondominioPermissions.Communities.Read);
        return endpoints;
    }
    private sealed record CreateCommunityRequest(Guid OrganizationId,string Code,string Name,string? TaxId,string Address);
    private sealed record UpdateCommunityRequest(string Name,string? TaxId,string Address,string? Type,string? Administrator,string? President);
    private sealed record TaxRequest(string LegalName,string TaxId,string Establishment,string EmissionPoint,bool AccountingObligation,string SriEnvironment);
    private sealed record NodeRequest(Guid? ParentId,StructureNodeType Type,string Code,string Name);
    private sealed record AreaRequest(string Code,string Name,bool Reservable);
    private sealed record BankRequest(string Bank,string Number,BankAccountType Type,string Currency,string Use,string? AccountingAccount);
    private sealed record ChargeRequest(int IssueDay,int DueDay,decimal InterestRate,string ApplicationOrder,string Currency,int FiscalYear);
    private sealed record AuthorityRequest(string Role,string PersonName,DateOnly StartsOn,string? SupportDocumentReference);
    private sealed record DocumentRequest(string DocumentType,string Name,string ExternalReference);
}
