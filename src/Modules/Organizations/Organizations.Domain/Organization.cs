using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Organizations.Domain;

public sealed class Organization : AggregateRoot
{
    private Organization(Guid id, string name, string? taxId)
        : base(id)
    {
        Name = name;
        TaxId = taxId;
    }

    public string Name { get; private set; }

    public string? TaxId { get; private set; }

    public bool IsActive { get; private set; } = true;

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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
