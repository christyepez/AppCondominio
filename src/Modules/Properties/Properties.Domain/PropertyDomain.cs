using AppCondominio.SharedKernel;

namespace AppCondominio.Modules.Properties.Domain;

public enum PropertyUnitStatus { Active = 1, Inactive = 2, UnderConstruction = 3 }
public enum PropertyRelationType { Parking = 1, Storage = 2, Other = 9 }
public enum PropertyAreaType { Main = 1, Covered = 2, Open = 3, Additional = 4, Computable = 5 }
public enum AliquotCalculationMethod { Area = 1, FixedPercentage = 2, Coefficient = 3, Mixed = 4, Manual = 9 }

public sealed class PropertyUnitType : AggregateRoot
{
    private PropertyUnitType(Guid id, Guid communityId, string code, string name) : base(id)
    { CommunityId = communityId; Code = code; Name = name; }
    public Guid CommunityId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; } = true;
    public static PropertyUnitType Create(Guid communityId, string code, string name)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("Community is required.", nameof(communityId));
        return new(Guid.NewGuid(), communityId, Required(code).ToLowerInvariant(), Required(name));
    }
    public void Deactivate() => IsActive = false;
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
}

public sealed class PropertyUnit : AggregateRoot
{
    private PropertyUnit(Guid id, Guid communityId, Guid typeId, string code, string location, decimal mainAreaM2) : base(id)
    { CommunityId = communityId; TypeId = typeId; Code = code; Location = location; MainAreaM2 = mainAreaM2; }
    public Guid CommunityId { get; private set; }
    public Guid TypeId { get; private set; }
    public string Code { get; private set; }
    public string Location { get; private set; }
    public PropertyUnitStatus Status { get; private set; } = PropertyUnitStatus.Active;
    public decimal MainAreaM2 { get; private set; }
    public string? Notes { get; private set; }
    public static PropertyUnit Create(Guid communityId, Guid typeId, string code, string location, decimal mainAreaM2, string? notes = null)
    {
        if (communityId == Guid.Empty || typeId == Guid.Empty) throw new ArgumentException("Community and unit type are required.");
        if (mainAreaM2 <= 0) throw new ArgumentOutOfRangeException(nameof(mainAreaM2));
        var unit = new PropertyUnit(Guid.NewGuid(), communityId, typeId, Required(code).ToLowerInvariant(), Required(location), mainAreaM2);
        unit.Notes = Optional(notes);
        return unit;
    }
    public void Update(string location, decimal mainAreaM2, string? notes, PropertyUnitStatus status)
    {
        if (mainAreaM2 <= 0) throw new ArgumentOutOfRangeException(nameof(mainAreaM2));
        Location = Required(location); MainAreaM2 = mainAreaM2; Notes = Optional(notes); Status = status;
    }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PropertyRelation : AggregateRoot
{
    private PropertyRelation(Guid id, Guid communityId, Guid mainUnitId, Guid relatedUnitId, PropertyRelationType relationType, DateOnly startsOn) : base(id)
    { CommunityId = communityId; MainUnitId = mainUnitId; RelatedUnitId = relatedUnitId; RelationType = relationType; StartsOn = startsOn; }
    public Guid CommunityId { get; private set; }
    public Guid MainUnitId { get; private set; }
    public Guid RelatedUnitId { get; private set; }
    public PropertyRelationType RelationType { get; private set; }
    public DateOnly StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public static PropertyRelation Create(Guid communityId, Guid mainUnitId, Guid relatedUnitId, PropertyRelationType relationType, DateOnly startsOn)
    {
        if (communityId == Guid.Empty || mainUnitId == Guid.Empty || relatedUnitId == Guid.Empty) throw new ArgumentException("Community and units are required.");
        if (mainUnitId == relatedUnitId) throw new ArgumentException("A unit cannot be related to itself.");
        return new(Guid.NewGuid(), communityId, mainUnitId, relatedUnitId, relationType, startsOn);
    }
    public void End(DateOnly endsOn)
    { if (endsOn < StartsOn) throw new ArgumentException("Invalid relation end date."); EndsOn = endsOn; }
}

public sealed class PropertyArea : AggregateRoot
{
    private PropertyArea(Guid id, Guid communityId, Guid unitId, PropertyAreaType type, decimal areaM2, bool isComputable, string? description) : base(id)
    { CommunityId = communityId; UnitId = unitId; Type = type; AreaM2 = areaM2; IsComputable = isComputable; Description = description; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public PropertyAreaType Type { get; private set; }
    public decimal AreaM2 { get; private set; }
    public bool IsComputable { get; private set; }
    public string? Description { get; private set; }
    public static PropertyArea Create(Guid communityId, Guid unitId, PropertyAreaType type, decimal areaM2, bool isComputable, string? description)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty) throw new ArgumentException("Community and unit are required.");
        if (areaM2 < 0) throw new ArgumentOutOfRangeException(nameof(areaM2));
        return new(Guid.NewGuid(), communityId, unitId, type, areaM2, isComputable, string.IsNullOrWhiteSpace(description) ? null : description.Trim());
    }
}

public sealed class AliquotVersion : AggregateRoot
{
    private AliquotVersion(Guid id, Guid communityId, Guid unitId, AliquotCalculationMethod method, decimal value, DateOnly validFrom, string reason) : base(id)
    { CommunityId = communityId; UnitId = unitId; Method = method; Value = value; ValidFrom = validFrom; Reason = reason; }
    public Guid CommunityId { get; private set; }
    public Guid UnitId { get; private set; }
    public AliquotCalculationMethod Method { get; private set; }
    public decimal Value { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
    public string Reason { get; private set; }
    public string? SupportDocumentReference { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAtUtc { get; private set; }
    public static AliquotVersion Create(Guid communityId, Guid unitId, AliquotCalculationMethod method, decimal value, DateOnly validFrom, string reason, string? supportDocumentReference)
    {
        if (communityId == Guid.Empty || unitId == Guid.Empty) throw new ArgumentException("Community and unit are required.");
        if (value < 0 || value > 100) throw new ArgumentOutOfRangeException(nameof(value), "Aliquot must be between 0 and 100.");
        var version = new AliquotVersion(Guid.NewGuid(), communityId, unitId, method, value, validFrom, Required(reason));
        version.SupportDocumentReference = string.IsNullOrWhiteSpace(supportDocumentReference) ? null : supportDocumentReference.Trim();
        return version;
    }
    public void Approve(string approvedBy, DateTimeOffset approvedAtUtc)
    { ApprovedBy = Required(approvedBy); ApprovedAtUtc = approvedAtUtc; }
    public void End(DateOnly validTo)
    { if (validTo < ValidFrom) throw new ArgumentException("Invalid aliquot validity."); ValidTo = validTo; }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Required value missing.") : value.Trim();
}

public static class AliquotCalculator
{
    public static decimal Calculate(AliquotCalculationMethod method, decimal unitComputableArea, decimal totalComputableArea, decimal coefficient, decimal fixedPercentage, decimal manualValue)
    {
        decimal value = method switch
        {
            AliquotCalculationMethod.Area => totalComputableArea <= 0 ? throw new ArgumentOutOfRangeException(nameof(totalComputableArea)) : unitComputableArea / totalComputableArea * 100m,
            AliquotCalculationMethod.FixedPercentage => fixedPercentage,
            AliquotCalculationMethod.Coefficient => coefficient,
            AliquotCalculationMethod.Mixed => totalComputableArea <= 0 ? throw new ArgumentOutOfRangeException(nameof(totalComputableArea)) : ((unitComputableArea / totalComputableArea * 100m) + coefficient) / 2m,
            AliquotCalculationMethod.Manual => manualValue,
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };
        if (value < 0 || value > 100) throw new InvalidOperationException("Calculated aliquot must be between 0 and 100.");
        return decimal.Round(value, 6, MidpointRounding.AwayFromZero);
    }
}
