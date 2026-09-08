using System.Security.Cryptography;
using System.Text;
using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.People.Domain;

public enum PersonKind { Natural = 1, LegalEntity = 2 }
public enum OccupancyRole { ResidentOwner = 1, Tenant = 2, Family = 3, Dependent = 4, ServiceStaff = 5 }
public enum ActivationStatus { Pending = 0, Approved = 1, Rejected = 2, Expired = 3 }
public enum AccessGrantStatus { Active = 1, Revoked = 2, Expired = 3 }

public sealed class Person : AggregateRoot
{
    private Person(Guid id, Guid communityId, PersonKind kind, string identification, string displayName) : base(id)
    { CommunityId = communityId; Kind = kind; Identification = identification; DisplayName = displayName; }
    public Guid CommunityId { get; private set; }
    public PersonKind Kind { get; private set; }
    public string Identification { get; private set; }
    public string DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? Address { get; private set; }
    public string? LegalRepresentative { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static Person Natural(Guid communityId, string identification, string names, string? email, string? phone, DateOnly? birthDate, string? address)
    {
        var person = CreateCore(communityId, PersonKind.Natural, identification, names);
        person.Email = Optional(email); person.Phone = Optional(phone); person.BirthDate = birthDate; person.Address = Optional(address);
        return person;
    }
    public static Person Legal(Guid communityId, string taxId, string legalName, string representative, string? email, string? phone, string? address)
    {
        var person = CreateCore(communityId, PersonKind.LegalEntity, taxId, legalName);
        person.LegalRepresentative = Required(representative); person.Email = Optional(email); person.Phone = Optional(phone); person.Address = Optional(address);
        return person;
    }
    public void Deactivate() => IsActive = false;
    private static Person CreateCore(Guid communityId, PersonKind kind, string identification, string displayName)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("Community is required.", nameof(communityId));
        return new(Guid.NewGuid(), communityId, kind, Required(identification), Required(displayName));
    }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class Ownership : AggregateRoot
{
    private Ownership(Guid id, Guid communityId, Guid unitId, Guid personId, decimal percentage, DateOnly startsOn, string acquisitionType) : base(id)
    { CommunityId = communityId; UnitId = unitId; PersonId = personId; Percentage = percentage; StartsOn = startsOn; AcquisitionType = acquisitionType; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid PersonId { get; private set; }
    public decimal Percentage { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public string AcquisitionType { get; private set; }
    public string? SupportDocumentReference { get; private set; }
    public static Ownership Create(Guid communityId, Guid unitId, Guid personId, decimal percentage, DateOnly startsOn, string acquisitionType, string? supportDocumentReference)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty || personId == Guid.Empty) throw new ArgumentException("Community, unit and person are required.");
        if (percentage <= 0 || percentage > 100) throw new ArgumentOutOfRangeException(nameof(percentage));
        var ownership = new Ownership(Guid.NewGuid(), communityId, unitId, personId, percentage, startsOn, Required(acquisitionType));
        ownership.SupportDocumentReference = Optional(supportDocumentReference);
        return ownership;
    }
    public void End(DateOnly endsOn) { if (endsOn < StartsOn) throw new ArgumentException("Invalid ownership end date."); EndsOn = endsOn; }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class LeaseContract : AggregateRoot
{
    private LeaseContract(Guid id, Guid communityId, Guid unitId, Guid tenantPersonId, Guid responsibleOwnerPersonId, DateOnly startsOn, DateOnly endsOn) : base(id)
    { CommunityId = communityId; UnitId = unitId; TenantPersonId = tenantPersonId; ResponsibleOwnerPersonId = responsibleOwnerPersonId; StartsOn = startsOn; EndsOn = endsOn; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid TenantPersonId { get; private set; }
    public Guid ResponsibleOwnerPersonId { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public string PermissionsCsv { get; private set; } = string.Empty;
    public string? ContractDocumentReference { get; private set; }
    public bool IsActive { get; private set; } = true;
    public static LeaseContract Create(Guid communityId, Guid unitId, Guid tenantPersonId, Guid responsibleOwnerPersonId, DateOnly startsOn, DateOnly endsOn, IEnumerable<string> permissions, string? contractDocumentReference)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty || tenantPersonId == Guid.Empty || responsibleOwnerPersonId == Guid.Empty) throw new ArgumentException("Required identifiers are missing.");
        if (endsOn < startsOn) throw new ArgumentException("Lease end date must be on or after start date.");
        var lease = new LeaseContract(Guid.NewGuid(), communityId, unitId, tenantPersonId, responsibleOwnerPersonId, startsOn, endsOn);
        lease.PermissionsCsv = NormalizePermissions(permissions); lease.ContractDocumentReference = Optional(contractDocumentReference);
        return lease;
    }
    public void Expire(DateOnly onDate) { if (onDate >= EndsOn) IsActive = false; }
    private static string NormalizePermissions(IEnumerable<string> permissions) => string.Join(',', permissions.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal).OrderBy(x => x));
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class Resident : AggregateRoot
{
    private Resident(Guid id, Guid communityId, Guid unitId, Guid personId, OccupancyRole role, DateOnly startsOn) : base(id)
    { CommunityId = communityId; UnitId = unitId; PersonId = personId; Role = role; StartsOn = startsOn; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid PersonId { get; private set; }
    public OccupancyRole Role { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public bool IsActive => EndsOn is null;
    public static Resident Create(Guid communityId, Guid unitId, Guid personId, OccupancyRole role, DateOnly startsOn) =>
        communityId == Guid.Empty || unitId == Guid.Empty || personId == Guid.Empty
            ? throw new ArgumentException("Community, unit and person are required.")
            : new(Guid.NewGuid(), communityId, unitId, personId, role, startsOn);
    public void End(DateOnly endsOn) { if (endsOn < StartsOn) throw new ArgumentException("Invalid resident end date."); EndsOn = endsOn; }
}

public sealed class UnitAccessCode : AggregateRoot
{
    private UnitAccessCode(Guid id, Guid communityId, Guid unitId, string codeHash, DateTimeOffset expiresAtUtc) : base(id)
    { CommunityId = communityId; UnitId = unitId; CodeHash = codeHash; ExpiresAtUtc = expiresAtUtc; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public string CodeHash { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? ConsumedAtUtc { get; private set; }
    public bool IsUsable(DateTimeOffset now) => ConsumedAtUtc is null && now <= ExpiresAtUtc;
    public static UnitAccessCode Create(Guid communityId, Guid unitId, string rawCode, DateTimeOffset expiresAtUtc)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty) throw new ArgumentException("Community and unit are required.");
        if (string.IsNullOrWhiteSpace(rawCode)) throw new ArgumentException("Activation code is required.");
        if (expiresAtUtc <= DateTimeOffset.UtcNow) throw new ArgumentException("Activation code expiry must be in the future.");
        return new(Guid.NewGuid(), communityId, unitId, Hash(rawCode), expiresAtUtc);
    }
    public bool Matches(string rawCode) => CryptographicOperations.FixedTimeEquals(Convert.FromHexString(CodeHash), Convert.FromHexString(Hash(rawCode)));
    public void Consume(DateTimeOffset now) { if (!IsUsable(now)) throw new InvalidOperationException("Activation code is expired or already used."); ConsumedAtUtc = now; }
    public static string Hash(string rawCode) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawCode.Trim())));
}

public sealed class ActivationRequest : AggregateRoot
{
    private ActivationRequest(Guid id, Guid communityId, Guid unitId, Guid personId, string externalUserId, OccupancyRole requestedRole) : base(id)
    { CommunityId = communityId; UnitId = unitId; PersonId = personId; ExternalUserId = externalUserId; RequestedRole = requestedRole; RequestedAtUtc = DateTimeOffset.UtcNow; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid PersonId { get; private set; }
    public string ExternalUserId { get; private set; }
    public OccupancyRole RequestedRole { get; private set; }
    public ActivationStatus Status { get; private set; } = ActivationStatus.Pending;
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }
    public string? DecidedBy { get; private set; }
    public string? DecisionReason { get; private set; }
    public static ActivationRequest Create(Guid communityId, Guid unitId, Guid personId, string externalUserId, OccupancyRole requestedRole)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty || personId == Guid.Empty) throw new ArgumentException("Community, unit and person are required.");
        if (string.IsNullOrWhiteSpace(externalUserId)) throw new ArgumentException("External user is required.");
        return new(Guid.NewGuid(), communityId, unitId, personId, externalUserId.Trim(), requestedRole);
    }
    public void Approve(string decidedBy) { EnsurePending(); Status = ActivationStatus.Approved; DecidedBy = Required(decidedBy); DecidedAtUtc = DateTimeOffset.UtcNow; }
    public void Reject(string decidedBy, string reason) { EnsurePending(); Status = ActivationStatus.Rejected; DecidedBy = Required(decidedBy); DecisionReason = Required(reason); DecidedAtUtc = DateTimeOffset.UtcNow; }
    private void EnsurePending() { if (Status != ActivationStatus.Pending) throw new InvalidOperationException("Activation request is already decided."); }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
}

public sealed class AccessGrant : AggregateRoot
{
    private AccessGrant(Guid id, Guid communityId, Guid unitId, Guid personId, string externalUserId, OccupancyRole role, DateTimeOffset startsAtUtc, DateTimeOffset? expiresAtUtc) : base(id)
    { CommunityId = communityId; UnitId = unitId; PersonId = personId; ExternalUserId = externalUserId; Role = role; StartsAtUtc = startsAtUtc; ExpiresAtUtc = expiresAtUtc; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid PersonId { get; private set; }
    public string ExternalUserId { get; private set; }
    public OccupancyRole Role { get; private set; }
    public DateTimeOffset StartsAtUtc { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public AccessGrantStatus Status { get; private set; } = AccessGrantStatus.Active;
    public string? RevocationReason { get; private set; }
    public static AccessGrant Create(Guid communityId, Guid unitId, Guid personId, string externalUserId, OccupancyRole role, DateTimeOffset startsAtUtc, DateTimeOffset? expiresAtUtc) => new(Guid.NewGuid(), communityId, unitId, personId, externalUserId.Trim(), role, startsAtUtc, expiresAtUtc);
    public bool ExpireIfDue(DateTimeOffset now) { if (Status == AccessGrantStatus.Active && ExpiresAtUtc is not null && now >= ExpiresAtUtc) { Status = AccessGrantStatus.Expired; return true; } return false; }
    public void Revoke(string reason) { if (Status != AccessGrantStatus.Active) return; Status = AccessGrantStatus.Revoked; RevocationReason = string.IsNullOrWhiteSpace(reason) ? "Revoked" : reason.Trim(); }
}

public sealed class FinancialResponsibility : AggregateRoot
{
    private FinancialResponsibility(Guid id, Guid communityId, Guid unitId, Guid personId, DateOnly startsOn) : base(id)
    { CommunityId = communityId; UnitId = unitId; PersonId = personId; StartsOn = startsOn; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid PersonId { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public static FinancialResponsibility Create(Guid communityId, Guid unitId, Guid personId, DateOnly startsOn) => new(Guid.NewGuid(), communityId, unitId, personId, startsOn);
    public void End(DateOnly endsOn) { if (endsOn < StartsOn) throw new ArgumentException("Invalid responsibility end date."); EndsOn = endsOn; }
}
