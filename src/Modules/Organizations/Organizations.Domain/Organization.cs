using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Organizations.Domain;

public sealed class Organization : AggregateRoot
{
    private Organization(Guid id, string name, string? taxId)
        : base(id)
    {
        Name = name;
        LegalName = name;
        CommercialName = name;
        TaxId = taxId;
    }

    public string Name { get; private set; }
    public string LegalName { get; private set; }
    public string CommercialName { get; private set; }
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? LegalRepresentative { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? SuspendedAtUtc { get; private set; }
    public string? SuspensionReason { get; private set; }

    public static Organization Create(string name, string? taxId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Organization name is required.", nameof(name));
        }

        Organization organization = new(Guid.NewGuid(), name.Trim(), NormalizeOptional(taxId));
        organization.Raise(new OrganizationCreatedDomainEvent(
            Guid.NewGuid(),
            organization.Id,
            DateTime.UtcNow));

        return organization;
    }

    public void UpdateProfile(string legalName, string commercialName, string? taxId, string? email,
        string? phone, string? address, string? legalRepresentative)
    {
        LegalName = Required(legalName, nameof(legalName));
        CommercialName = Required(commercialName, nameof(commercialName));
        Name = CommercialName;
        TaxId = NormalizeOptional(taxId);
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
        Address = NormalizeOptional(address);
        LegalRepresentative = NormalizeOptional(legalRepresentative);
    }

    public void Suspend(string reason, DateTimeOffset now)
    {
        if (!IsActive) return;
        IsActive = false;
        SuspendedAtUtc = now;
        SuspensionReason = Required(reason, nameof(reason));
    }

    public void Reactivate()
    {
        IsActive = true;
        SuspendedAtUtc = null;
        SuspensionReason = null;
    }

    private static string Required(string value, string field) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{field} is required.") : value.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
