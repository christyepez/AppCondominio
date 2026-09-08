using AppCondominio.Contracts.Security;
using AppCondominio.Modules.Organizations.Application.Saas;
using AppCondominio.Modules.Organizations.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppCondominio.Modules.Organizations.Api;

public static class SaasEndpoints
{
    public static IEndpointRouteBuilder MapSaasEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/saas").WithTags("SaaS Administration").RequireAuthorization();

        group.MapPost("/plans", async (CreatePlanRequest r, SaasService s, CancellationToken ct) =>
            Results.Created("/api/saas/plans", await s.CreatePlanAsync(new(r.Code, r.Name, r.MaxUnits, r.MaxUsers,
                r.MaxStorageMb, r.Price, r.Currency, r.BillingPeriod, r.Modules ?? []), ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapGet("/plans", async (SaasService s, CancellationToken ct) => Results.Ok(await s.ListPlansAsync(ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Read);

        group.MapPost("/subscriptions", async (CreateSubscriptionRequest r, SaasService s, CancellationToken ct) =>
            Results.Created("/api/saas/subscriptions", await s.SubscribeAsync(new(r.OrganizationId, r.PlanId,
                r.StartsAtUtc ?? DateTimeOffset.UtcNow, r.CommercialNotes), ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapPut("/organizations/{organizationId:guid}/profile", async (Guid organizationId, OrganizationProfileRequest r,
            SaasService s, CancellationToken ct) =>
        {
            await s.UpdateOrganizationProfileAsync(new(organizationId, r.LegalName, r.CommercialName, r.TaxId,
                r.Email, r.Phone, r.Address, r.LegalRepresentative), ct);
            return Results.NoContent();
        }).RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapPut("/organizations/{organizationId:guid}/branding", async (Guid organizationId, BrandingRequest r,
            SaasService s, CancellationToken ct) => Results.Ok(await s.UpsertBrandingAsync(new(organizationId,
                r.ProductName, r.LogoUrl, r.FaviconUrl, r.PrimaryColor, r.SecondaryColor, r.ContactEmail, r.ContactPhone), ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapPut("/organizations/{organizationId:guid}/database-profile", async (Guid organizationId,
            DatabaseProfileRequest r, SaasService s, CancellationToken ct) => Results.Ok(await s.ConfigureDatabaseAsync(
                new(organizationId, r.Strategy, r.ConnectionSecretReference, r.MigrationStatus), ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapPost("/organizations/{organizationId:guid}/suspend", async (Guid organizationId, SuspendRequest r,
            SaasService s, CancellationToken ct) => { await s.SuspendAsync(organizationId, r.Reason, ct); return Results.NoContent(); })
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapPost("/organizations/{organizationId:guid}/reactivate", async (Guid organizationId,
            SaasService s, CancellationToken ct) => { await s.ReactivateAsync(organizationId, ct); return Results.NoContent(); })
            .RequireAuthorization(AppCondominioPermissions.Saas.Manage);

        group.MapGet("/organizations/{organizationId:guid}/entitlements", async (Guid organizationId,
            SaasService s, CancellationToken ct) => Results.Ok(await s.GetEntitlementsAsync(organizationId, ct)))
            .RequireAuthorization(AppCondominioPermissions.Saas.Read);

        return endpoints;
    }

    private sealed record CreatePlanRequest(string Code, string Name, int MaxUnits, int MaxUsers, long MaxStorageMb,
        decimal Price, string Currency, BillingPeriod BillingPeriod, string[]? Modules);
    private sealed record CreateSubscriptionRequest(Guid OrganizationId, Guid PlanId, DateTimeOffset? StartsAtUtc, string? CommercialNotes);
    private sealed record OrganizationProfileRequest(string LegalName, string CommercialName, string? TaxId, string? Email,
        string? Phone, string? Address, string? LegalRepresentative);
    private sealed record BrandingRequest(string ProductName, string? LogoUrl, string? FaviconUrl, string PrimaryColor,
        string SecondaryColor, string? ContactEmail, string? ContactPhone);
    private sealed record DatabaseProfileRequest(TenantDatabaseStrategy Strategy, string? ConnectionSecretReference, string MigrationStatus);
    private sealed record SuspendRequest(string Reason);
}
