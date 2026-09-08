using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Organizations.Domain;

public enum BillingPeriod { Monthly = 1, Quarterly = 3, Annual = 12 }
public enum SubscriptionStatus { Draft = 0, Active = 1, Suspended = 2, Cancelled = 3, Expired = 4 }
public enum TenantDatabaseStrategy { Shared = 0, Dedicated = 1 }

public sealed class CommercialPlan : AggregateRoot
{
    private CommercialPlan(Guid id, string code, string name, int maxUnits, int maxUsers, long maxStorageMb,
        decimal price, string currency, BillingPeriod billingPeriod, string modulesCsv) : base(id)
    {
        Code = code; Name = name; MaxUnits = maxUnits; MaxUsers = maxUsers; MaxStorageMb = maxStorageMb;
        Price = price; Currency = currency; BillingPeriod = billingPeriod; ModulesCsv = modulesCsv;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public int MaxUnits { get; private set; }
    public int MaxUsers { get; private set; }
    public long MaxStorageMb { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public BillingPeriod BillingPeriod { get; private set; }
    public string ModulesCsv { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyCollection<string> Modules => ParseModules(ModulesCsv);

    public static CommercialPlan Create(string code, string name, int maxUnits, int maxUsers, long maxStorageMb,
        decimal price, string currency, BillingPeriod billingPeriod, IEnumerable<string> modules)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Plan code and name are required.");
        if (maxUnits <= 0 || maxUsers <= 0 || maxStorageMb <= 0) throw new ArgumentOutOfRangeException(nameof(maxUnits), "Plan limits must be positive.");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        var normalizedModules = NormalizeModules(modules);
        return new CommercialPlan(Guid.NewGuid(), code.Trim().ToLowerInvariant(), name.Trim(), maxUnits, maxUsers,
            maxStorageMb, price, NormalizeCurrency(currency), billingPeriod, string.Join(',', normalizedModules));
    }

    public bool AllowsModule(string moduleCode) => Modules.Contains(moduleCode.Trim().ToLowerInvariant(), StringComparer.Ordinal);
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "USD" : value.Trim().ToUpperInvariant();
    private static string[] NormalizeModules(IEnumerable<string> modules) => modules
        .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal).OrderBy(x => x).ToArray();
    private static IReadOnlyCollection<string> ParseModules(string csv) => string.IsNullOrWhiteSpace(csv)
        ? Array.Empty<string>() : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public sealed class Subscription : AggregateRoot
{
    private Subscription(Guid id, Guid organizationId, Guid planId, DateTimeOffset startsAtUtc, DateTimeOffset renewsAtUtc,
        string modulesCsv) : base(id)
    {
        OrganizationId = organizationId; PlanId = planId; StartsAtUtc = startsAtUtc; RenewsAtUtc = renewsAtUtc;
        ModulesCsv = modulesCsv; Status = SubscriptionStatus.Active;
    }

    public Guid OrganizationId { get; private set; }
    public Guid PlanId { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset RenewsAtUtc { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string ModulesCsv { get; private set; }
    public string? CommercialNotes { get; private set; }
    public IReadOnlyCollection<string> Modules => string.IsNullOrWhiteSpace(ModulesCsv)
        ? Array.Empty<string>() : ModulesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static Subscription Create(Guid organizationId, CommercialPlan plan, DateTimeOffset startsAtUtc, string? commercialNotes = null)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization is required.", nameof(organizationId));
        ArgumentNullException.ThrowIfNull(plan);
        var renewsAt = startsAtUtc.AddMonths((int)plan.BillingPeriod);
        var subscription = new Subscription(Guid.NewGuid(), organizationId, plan.Id, startsAtUtc, renewsAt, plan.ModulesCsv)
        { CommercialNotes = NormalizeOptional(commercialNotes) };
        return subscription;
    }

    public void Suspend() { if (Status == SubscriptionStatus.Cancelled) throw new InvalidOperationException("Cancelled subscription cannot be suspended."); Status = SubscriptionStatus.Suspended; }
    public void Reactivate() { if (Status == SubscriptionStatus.Cancelled) throw new InvalidOperationException("Cancelled subscription cannot be reactivated."); Status = SubscriptionStatus.Active; }
    public bool AllowsModule(string moduleCode) => Status == SubscriptionStatus.Active && Modules.Contains(moduleCode.Trim().ToLowerInvariant(), StringComparer.Ordinal);
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class BrandSettings : AggregateRoot
{
    private BrandSettings(Guid id, Guid organizationId, string productName) : base(id) { OrganizationId = organizationId; ProductName = productName; }
    public Guid OrganizationId { get; private set; }
    public string ProductName { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? FaviconUrl { get; private set; }
    public string PrimaryColor { get; private set; } = "#3C235F";
    public string SecondaryColor { get; private set; } = "#F28C28";
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    public static BrandSettings Create(Guid organizationId, string productName) => new(Guid.NewGuid(), organizationId,
        string.IsNullOrWhiteSpace(productName) ? "Conjunto al Día" : productName.Trim());
    public void Update(string productName, string? logoUrl, string? faviconUrl, string primaryColor, string secondaryColor,
        string? contactEmail, string? contactPhone)
    {
        ProductName = string.IsNullOrWhiteSpace(productName) ? throw new ArgumentException("Product name is required.") : productName.Trim();
        LogoUrl = NormalizeOptional(logoUrl); FaviconUrl = NormalizeOptional(faviconUrl);
        PrimaryColor = NormalizeColor(primaryColor); SecondaryColor = NormalizeColor(secondaryColor);
        ContactEmail = NormalizeOptional(contactEmail); ContactPhone = NormalizeOptional(contactPhone);
    }
    private static string NormalizeColor(string value) => string.IsNullOrWhiteSpace(value) || !value.Trim().StartsWith('#') ? throw new ArgumentException("Color must be a hex value.") : value.Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenantDatabaseProfile : AggregateRoot
{
    private TenantDatabaseProfile(Guid id, Guid organizationId, TenantDatabaseStrategy strategy) : base(id) { OrganizationId = organizationId; Strategy = strategy; }
    public Guid OrganizationId { get; private set; }
    public TenantDatabaseStrategy Strategy { get; private set; }
    public string? ConnectionSecretReference { get; private set; }
    public string MigrationStatus { get; private set; } = "NotRequired";

    public static TenantDatabaseProfile Shared(Guid organizationId) => new(Guid.NewGuid(), organizationId, TenantDatabaseStrategy.Shared);
    public void Configure(TenantDatabaseStrategy strategy, string? connectionSecretReference, string migrationStatus)
    {
        if (strategy == TenantDatabaseStrategy.Dedicated && string.IsNullOrWhiteSpace(connectionSecretReference))
            throw new ArgumentException("Dedicated database requires a secret reference, never a raw connection string.");
        Strategy = strategy;
        ConnectionSecretReference = string.IsNullOrWhiteSpace(connectionSecretReference) ? null : connectionSecretReference.Trim();
        MigrationStatus = string.IsNullOrWhiteSpace(migrationStatus) ? "Pending" : migrationStatus.Trim();
    }
}
