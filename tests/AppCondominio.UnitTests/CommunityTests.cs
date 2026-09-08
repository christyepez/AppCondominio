using AppCondominio.Modules.Communities.Domain;

namespace AppCondominio.UnitTests;

public sealed class CommunityTests
{
    [Fact]
    public void Community_requires_organization()
    {
        Assert.Throws<ArgumentException>(() => Community.Create(Guid.Empty, "demo", "Demo", null, "Quito"));
    }

    [Fact]
    public void Community_normalizes_code_and_updates_profile()
    {
        var organizationId = Guid.NewGuid();
        var community = Community.Create(organizationId, " DEMO-01 ", "Conjunto Demo", "1790012345001", "Quito");

        community.Update("Conjunto Demo Norte", "1790012345001", "Quito Norte", "Condominio", "Admin Demo", "Presidente Demo");

        Assert.Equal("demo-01", community.Code);
        Assert.Equal("Conjunto Demo Norte", community.Name);
        Assert.Equal(CommunityStatus.Active, community.Status);
        Assert.Equal("Admin Demo", community.AdministratorName);
    }

    [Fact]
    public void Charge_settings_validate_issue_day()
    {
        var settings = ChargeSettings.Create(Guid.NewGuid());
        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Update(31, 10, 0.02m, "interest,oldest,current", "USD", 2026));
    }

    [Fact]
    public void Community_document_requires_external_content_reference()
    {
        Assert.Throws<ArgumentException>(() => CommunityDocument.Register(Guid.NewGuid(), "regulation", "Reglamento", ""));
    }
}
