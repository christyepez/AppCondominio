using AppCondominio.Contracts.Security;
using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.People.Api;

public static class PeopleEndpoints
{
    public static IEndpointRouteBuilder MapPeopleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/people").WithTags("People").RequireAuthorization();

        group.MapPost("/natural",async(NaturalPersonRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/natural",new{id=await s.CreateNaturalPersonAsync(r.CommunityId,r.Identification,r.Names,r.Email,r.Phone,r.BirthDate,r.Address,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/legal",async(LegalPersonRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/legal",new{id=await s.CreateLegalPersonAsync(r.CommunityId,r.TaxId,r.LegalName,r.Representative,r.Email,r.Phone,r.Address,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/ownerships",async(OwnershipRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/ownerships",new{id=await s.AddOwnershipAsync(r.CommunityId,r.UnitId,r.PersonId,r.Percentage,r.StartsOn,r.AcquisitionType,r.SupportDocumentReference,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/leases",async(LeaseRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/leases",new{id=await s.CreateLeaseAsync(r.CommunityId,r.UnitId,r.TenantPersonId,r.ResponsibleOwnerPersonId,r.StartsOn,r.EndsOn,r.Permissions,r.ContractDocumentReference,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/residents",async(ResidentRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/residents",new{id=await s.AddResidentAsync(r.CommunityId,r.UnitId,r.PersonId,r.Role,r.StartsOn,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/access-codes",async(AccessCodeRequest r,PeopleService s,CancellationToken ct)=>Results.Created("/api/people/access-codes",new{id=await s.CreateAccessCodeAsync(r.CommunityId,r.UnitId,r.RawCode,r.ExpiresAtUtc,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/activations",async(ActivationRequestDto r,PeopleService s,CancellationToken ct)=>Results.Accepted("/api/people/activations",new{id=await s.RequestActivationAsync(r.CommunityId,r.UnitId,r.PersonId,r.ExternalUserId,r.RequestedRole,r.RawCode,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/activations/{requestId:guid}/approve",async(Guid requestId,ActivationDecisionRequest r,PeopleService s,CancellationToken ct)=>Results.Ok(new{grantId=await s.ApproveActivationAsync(requestId,r.DecidedBy,r.ExpiresAtUtc,ct)})).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPost("/activations/{requestId:guid}/reject",async(Guid requestId,RejectActivationRequest r,PeopleService s,CancellationToken ct)=>{await s.RejectActivationAsync(requestId,r.DecidedBy,r.Reason,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapPut("/financial-responsibility",async(FinancialResponsibilityRequest r,PeopleService s,CancellationToken ct)=>{await s.SetFinancialResponsibilityAsync(r.CommunityId,r.UnitId,r.PersonId,r.StartsOn,ct);return Results.NoContent();}).RequireAuthorization(AppCondominioPermissions.People.Manage);
        group.MapGet("/communities/{communityId:guid}/units/{unitId:guid}",async(Guid communityId,Guid unitId,PeopleService s,CancellationToken ct)=>Results.Ok(await s.GetUnitRecordAsync(communityId,unitId,ct))).RequireAuthorization(AppCondominioPermissions.People.Read);
        group.MapPost("/communities/{communityId:guid}/expire-access",async(Guid communityId,ExpireAccessRequest r,PeopleService s,CancellationToken ct)=>Results.Ok(await s.ExpireTenantAccessAsync(communityId,r.NowUtc??DateTimeOffset.UtcNow,ct))).RequireAuthorization(AppCondominioPermissions.People.Manage);
        return endpoints;
    }

    private sealed record NaturalPersonRequest(Guid CommunityId,string Identification,string Names,string? Email,string? Phone,DateOnly? BirthDate,string? Address);
    private sealed record LegalPersonRequest(Guid CommunityId,string TaxId,string LegalName,string Representative,string? Email,string? Phone,string? Address);
    private sealed record OwnershipRequest(Guid CommunityId,Guid UnitId,Guid PersonId,decimal Percentage,DateOnly StartsOn,string AcquisitionType,string? SupportDocumentReference);
    private sealed record LeaseRequest(Guid CommunityId,Guid UnitId,Guid TenantPersonId,Guid ResponsibleOwnerPersonId,DateOnly StartsOn,DateOnly EndsOn,string[] Permissions,string? ContractDocumentReference);
    private sealed record ResidentRequest(Guid CommunityId,Guid UnitId,Guid PersonId,OccupancyRole Role,DateOnly StartsOn);
    private sealed record AccessCodeRequest(Guid CommunityId,Guid UnitId,string RawCode,DateTimeOffset ExpiresAtUtc);
    private sealed record ActivationRequestDto(Guid CommunityId,Guid UnitId,Guid PersonId,string ExternalUserId,OccupancyRole RequestedRole,string RawCode);
    private sealed record ActivationDecisionRequest(string DecidedBy,DateTimeOffset? ExpiresAtUtc);
    private sealed record RejectActivationRequest(string DecidedBy,string Reason);
    private sealed record FinancialResponsibilityRequest(Guid CommunityId,Guid UnitId,Guid PersonId,DateOnly StartsOn);
    private sealed record ExpireAccessRequest(DateTimeOffset? NowUtc);
}
