using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.Saas;

public interface ISaasRepository
{
    Task<Organization?> GetOrganizationAsync(Guid id, CancellationToken ct);
    Task<CommercialPlan?> GetPlanAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<CommercialPlan>> ListPlansAsync(CancellationToken ct);
    Task<Subscription?> GetSubscriptionByOrganizationAsync(Guid organizationId, CancellationToken ct);
    Task<BrandSettings?> GetBrandingAsync(Guid organizationId, CancellationToken ct);
    Task<TenantDatabaseProfile?> GetDatabaseProfileAsync(Guid organizationId, CancellationToken ct);
    Task AddPlanAsync(CommercialPlan plan, CancellationToken ct);
    Task AddSubscriptionAsync(Subscription subscription, CancellationToken ct);
    Task AddBrandingAsync(BrandSettings branding, CancellationToken ct);
    Task AddDatabaseProfileAsync(TenantDatabaseProfile profile, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record CreatePlanCommand(string Code, string Name, int MaxUnits, int MaxUsers, long MaxStorageMb,
    decimal Price, string Currency, BillingPeriod BillingPeriod, string[] Modules);
public sealed record PlanResult(Guid Id, string Code, string Name, int MaxUnits, int MaxUsers, long MaxStorageMb,
    decimal Price, string Currency, BillingPeriod BillingPeriod, IReadOnlyCollection<string> Modules, bool IsActive);
public sealed record CreateSubscriptionCommand(Guid OrganizationId, Guid PlanId, DateTimeOffset StartsAtUtc, string? CommercialNotes);
public sealed record SubscriptionResult(Guid Id, Guid OrganizationId, Guid PlanId, DateTimeOffset StartsAtUtc,
    DateTimeOffset RenewsAtUtc, SubscriptionStatus Status, IReadOnlyCollection<string> Modules);
public sealed record UpdateBrandingCommand(Guid OrganizationId, string ProductName, string? LogoUrl, string? FaviconUrl,
    string PrimaryColor, string SecondaryColor, string? ContactEmail, string? ContactPhone);
public sealed record BrandingResult(Guid OrganizationId, string ProductName, string? LogoUrl, string? FaviconUrl,
    string PrimaryColor, string SecondaryColor, string? ContactEmail, string? ContactPhone);
public sealed record UpdateOrganizationProfileCommand(Guid OrganizationId, string LegalName, string CommercialName,
    string? TaxId, string? Email, string? Phone, string? Address, string? LegalRepresentative);
public sealed record ConfigureDatabaseCommand(Guid OrganizationId, TenantDatabaseStrategy Strategy,
    string? ConnectionSecretReference, string MigrationStatus);
public sealed record DatabaseProfileResult(Guid OrganizationId, TenantDatabaseStrategy Strategy,
    string? ConnectionSecretReference, string MigrationStatus);
public sealed record EntitlementResult(Guid OrganizationId, bool OrganizationActive, SubscriptionStatus? SubscriptionStatus,
    int MaxUnits, int MaxUsers, long MaxStorageMb, IReadOnlyCollection<string> Modules, DateTimeOffset? RenewsAtUtc);
public sealed record SaasDashboardResult(int Organizations, int ActiveOrganizations, int ActiveSubscriptions,
    int SuspendedSubscriptions, int Plans, int ActivePlans, int ExpiringWithin30Days);

public sealed class SaasService(ISaasRepository repository)
{
    public async Task<PlanResult> CreatePlanAsync(CreatePlanCommand command, CancellationToken ct)
    {
        var plan = CommercialPlan.Create(command.Code, command.Name, command.MaxUnits, command.MaxUsers,
            command.MaxStorageMb, command.Price, command.Currency, command.BillingPeriod, command.Modules);
        await repository.AddPlanAsync(plan, ct);
        await repository.SaveChangesAsync(ct);
        return Map(plan);
    }

    public async Task<IReadOnlyCollection<PlanResult>> ListPlansAsync(CancellationToken ct) =>
        (await repository.ListPlansAsync(ct)).Select(Map).ToArray();

    public async Task<SubscriptionResult> SubscribeAsync(CreateSubscriptionCommand command, CancellationToken ct)
    {
        var organization = await RequireOrganization(command.OrganizationId, ct);
        if (!organization.IsActive) throw new InvalidOperationException("Suspended organization cannot receive a new subscription.");
        if (await repository.GetSubscriptionByOrganizationAsync(command.OrganizationId, ct) is not null)
            throw new InvalidOperationException("Organization already has a subscription.");
        var plan = await repository.GetPlanAsync(command.PlanId, ct) ?? throw new KeyNotFoundException("Plan was not found.");
        if (!plan.IsActive) throw new InvalidOperationException("Inactive plan cannot be subscribed.");
        var subscription = Subscription.Create(command.OrganizationId, plan, command.StartsAtUtc, command.CommercialNotes);
        await repository.AddSubscriptionAsync(subscription, ct);
        await repository.SaveChangesAsync(ct);
        return Map(subscription);
    }

    public async Task<BrandingResult> UpsertBrandingAsync(UpdateBrandingCommand command, CancellationToken ct)
    {
        await RequireOrganization(command.OrganizationId, ct);
        var branding = await repository.GetBrandingAsync(command.OrganizationId, ct);
        if (branding is null)
        {
            branding = BrandSettings.Create(command.OrganizationId, command.ProductName);
            await repository.AddBrandingAsync(branding, ct);
        }
        branding.Update(command.ProductName, command.LogoUrl, command.FaviconUrl, command.PrimaryColor,
            command.SecondaryColor, command.ContactEmail, command.ContactPhone);
        await repository.SaveChangesAsync(ct);
        return new BrandingResult(branding.OrganizationId, branding.ProductName, branding.LogoUrl, branding.FaviconUrl,
            branding.PrimaryColor, branding.SecondaryColor, branding.ContactEmail, branding.ContactPhone);
    }

    public async Task UpdateOrganizationProfileAsync(UpdateOrganizationProfileCommand command, CancellationToken ct)
    {
        var organization = await RequireOrganization(command.OrganizationId, ct);
        organization.UpdateProfile(command.LegalName, command.CommercialName, command.TaxId, command.Email,
            command.Phone, command.Address, command.LegalRepresentative);
        await repository.SaveChangesAsync(ct);
    }

    public async Task SuspendAsync(Guid organizationId, string reason, CancellationToken ct)
    {
        var organization = await RequireOrganization(organizationId, ct);
        organization.Suspend(reason, DateTimeOffset.UtcNow);
        var subscription = await repository.GetSubscriptionByOrganizationAsync(organizationId, ct);
        subscription?.Suspend();
        await repository.SaveChangesAsync(ct);
    }

    public async Task ReactivateAsync(Guid organizationId, CancellationToken ct)
    {
        var organization = await RequireOrganization(organizationId, ct);
        organization.Reactivate();
        var subscription = await repository.GetSubscriptionByOrganizationAsync(organizationId, ct);
        subscription?.Reactivate();
        await repository.SaveChangesAsync(ct);
    }

    public async Task<DatabaseProfileResult> ConfigureDatabaseAsync(ConfigureDatabaseCommand command, CancellationToken ct)
    {
        await RequireOrganization(command.OrganizationId, ct);
        var profile = await repository.GetDatabaseProfileAsync(command.OrganizationId, ct);
        if (profile is null)
        {
            profile = TenantDatabaseProfile.Shared(command.OrganizationId);
            await repository.AddDatabaseProfileAsync(profile, ct);
        }
        profile.Configure(command.Strategy, command.ConnectionSecretReference, command.MigrationStatus);
        await repository.SaveChangesAsync(ct);
        return new(profile.OrganizationId, profile.Strategy, profile.ConnectionSecretReference, profile.MigrationStatus);
    }

    public async Task<EntitlementResult> GetEntitlementsAsync(Guid organizationId, CancellationToken ct)
    {
        var organization = await RequireOrganization(organizationId, ct);
        var subscription = await repository.GetSubscriptionByOrganizationAsync(organizationId, ct);
        if (subscription is null) return new(organizationId, organization.IsActive, null, 0, 0, 0, Array.Empty<string>(), null);
        var plan = await repository.GetPlanAsync(subscription.PlanId, ct) ?? throw new InvalidOperationException("Subscription plan is missing.");
        return new(organizationId, organization.IsActive, subscription.Status, plan.MaxUnits, plan.MaxUsers,
            plan.MaxStorageMb, subscription.Modules, subscription.RenewsAtUtc);
    }

    public static void ValidateUsage(EntitlementResult entitlement, int units, int users, long storageMb, string? module)
    {
        if (!entitlement.OrganizationActive || entitlement.SubscriptionStatus != SubscriptionStatus.Active)
            throw new InvalidOperationException("Tenant subscription is not active.");
        if (units > entitlement.MaxUnits) throw new InvalidOperationException("Unit limit exceeded.");
        if (users > entitlement.MaxUsers) throw new InvalidOperationException("User limit exceeded.");
        if (storageMb > entitlement.MaxStorageMb) throw new InvalidOperationException("Storage limit exceeded.");
        if (!string.IsNullOrWhiteSpace(module) && !entitlement.Modules.Contains(module.Trim().ToLowerInvariant(), StringComparer.Ordinal))
            throw new InvalidOperationException("Module is not included in the subscription.");
    }

    private async Task<Organization> RequireOrganization(Guid id, CancellationToken ct) =>
        await repository.GetOrganizationAsync(id, ct) ?? throw new KeyNotFoundException("Organization was not found.");
    private static PlanResult Map(CommercialPlan x) => new(x.Id, x.Code, x.Name, x.MaxUnits, x.MaxUsers,
        x.MaxStorageMb, x.Price, x.Currency, x.BillingPeriod, x.Modules, x.IsActive);
    private static SubscriptionResult Map(Subscription x) => new(x.Id, x.OrganizationId, x.PlanId, x.StartsAtUtc,
        x.RenewsAtUtc, x.Status, x.Modules);
}
