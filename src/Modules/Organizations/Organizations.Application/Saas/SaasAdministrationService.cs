using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.Modules.Organizations.Application.Saas;

public interface ISaasMetricsRepository
{
    Task<IReadOnlyCollection<Organization>> ListOrganizationsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<Subscription>> ListSubscriptionsAsync(CancellationToken ct);
    Task<IReadOnlyCollection<CommercialPlan>> ListPlansAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record UsageAssessment(bool Allowed, IReadOnlyCollection<string> Warnings);

public sealed class SaasAdministrationService(
    ISaasRepository repository,
    ISaasMetricsRepository metrics)
{
    public async Task<SubscriptionResult> UpdateModulesAsync(Guid organizationId, string[] modules, CancellationToken ct)
    {
        var subscription = await repository.GetSubscriptionByOrganizationAsync(organizationId, ct)
            ?? throw new KeyNotFoundException("Subscription was not found.");
        var plan = await repository.GetPlanAsync(subscription.PlanId, ct)
            ?? throw new InvalidOperationException("Subscription plan is missing.");

        subscription.UpdateModules(plan, modules);
        await repository.SaveChangesAsync(ct);
        return new(subscription.Id, subscription.OrganizationId, subscription.PlanId, subscription.StartsAtUtc,
            subscription.RenewsAtUtc, subscription.Status, subscription.Modules);
    }

    public async Task<UsageAssessment> AssessUsageAsync(Guid organizationId, int units, int users, long storageMb,
        string? requestedModule, CancellationToken ct)
    {
        var entitlement = await new SaasService(repository).GetEntitlementsAsync(organizationId, ct);
        var warnings = new List<string>();
        if (!entitlement.OrganizationActive || entitlement.SubscriptionStatus != SubscriptionStatus.Active)
            return new(false, ["Tenant subscription is not active."]);
        if (units > entitlement.MaxUnits || users > entitlement.MaxUsers || storageMb > entitlement.MaxStorageMb)
            return new(false, ["One or more subscription limits are exceeded."]);
        if (!string.IsNullOrWhiteSpace(requestedModule) && !entitlement.Modules.Contains(requestedModule.Trim().ToLowerInvariant(), StringComparer.Ordinal))
            return new(false, ["Requested module is not contracted."]);

        AddThresholdWarning(warnings, units, entitlement.MaxUnits, "units");
        AddThresholdWarning(warnings, users, entitlement.MaxUsers, "users");
        AddThresholdWarning(warnings, storageMb, entitlement.MaxStorageMb, "storage");
        return new(true, warnings);
    }

    public async Task<SaasDashboardResult> GetDashboardAsync(CancellationToken ct)
    {
        var organizations = await metrics.ListOrganizationsAsync(ct);
        var subscriptions = await metrics.ListSubscriptionsAsync(ct);
        var plans = await metrics.ListPlansAsync(ct);
        var now = DateTimeOffset.UtcNow;
        return new(
            organizations.Count,
            organizations.Count(x => x.IsActive),
            subscriptions.Count(x => x.Status == SubscriptionStatus.Active),
            subscriptions.Count(x => x.Status == SubscriptionStatus.Suspended),
            plans.Count,
            plans.Count(x => x.IsActive),
            subscriptions.Count(x => x.Status == SubscriptionStatus.Active && x.RenewsAtUtc >= now && x.RenewsAtUtc <= now.AddDays(30)));
    }

    private static void AddThresholdWarning(List<string> warnings, long used, long limit, string label)
    {
        if (limit <= 0) return;
        var percent = (decimal)used / limit;
        if (percent >= 0.9m) warnings.Add($"{label} usage is at or above 90% of the contracted limit.");
        else if (percent >= 0.8m) warnings.Add($"{label} usage is at or above 80% of the contracted limit.");
    }
}
