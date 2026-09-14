using AppCondominio.Modules.Properties.Domain;

namespace AppCondominio.UnitTests;

public sealed class PropertyTests
{
    [Fact]
    public void Area_method_calculates_percentage()
    {
        decimal value = AliquotCalculator.Calculate(AliquotCalculationMethod.Area, 80m, 8000m, 0m, 0m, 0m);
        Assert.Equal(1m, value);
    }

    [Fact]
    public void Unit_requires_positive_main_area()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PropertyUnit.Create(Guid.NewGuid(), Guid.NewGuid(), "A-001", "Tower A", 0m));
    }

    [Fact]
    public void Unit_cannot_relate_to_itself()
    {
        Guid unitId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => PropertyRelation.Create(Guid.NewGuid(), unitId, unitId, PropertyRelationType.Parking, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Manual_aliquot_cannot_exceed_100()
    {
        Assert.Throws<InvalidOperationException>(() => AliquotCalculator.Calculate(AliquotCalculationMethod.Manual, 0m, 0m, 0m, 0m, 101m));
    }
}
