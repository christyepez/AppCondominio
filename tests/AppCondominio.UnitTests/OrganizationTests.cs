using AppCondominio.Modules.Organizations.Domain;

namespace AppCondominio.UnitTests;

public sealed class OrganizationTests
{
    [Fact]
    public void Create_trims_values_and_raises_created_event()
    {
        var organization = Organization.Create("  Administración Norte  ", " 1790012345001 ");

        Assert.Equal("Administración Norte", organization.Name);
        Assert.Equal("1790012345001", organization.TaxId);
        Assert.True(organization.IsActive);
        Assert.Single(organization.DomainEvents);
    }

    [Fact]
    public void Create_rejects_empty_name()
    {
        Assert.Throws<ArgumentException>(() => Organization.Create("   "));
    }
}
