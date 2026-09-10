using AppCondominio.Modules.Properties.Domain;

namespace AppCondominio.Modules.Properties.Application;

public interface IPropertyRepository
{
    Task AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class;
    Task<PropertyUnitType?> GetTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<PropertyUnit?> GetUnitAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PropertyUnit>> ListUnitsAsync(Guid communityId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PropertyArea>> ListAreasAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AliquotVersion>> ListAliquotsAsync(Guid communityId, CancellationToken cancellationToken);
    Task<bool> TypeCodeExistsAsync(Guid communityId, string code, CancellationToken cancellationToken);
    Task<bool> UnitCodeExistsAsync(Guid communityId, string code, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CreateUnitTypeCommand(Guid CommunityId, string Code, string Name);
public sealed record CreateUnitCommand(Guid CommunityId, Guid TypeId, string Code, string Location, decimal MainAreaM2, string? Notes);
public sealed record AddAreaCommand(Guid CommunityId, Guid UnitId, PropertyAreaType Type, decimal AreaM2, bool IsComputable, string? Description);
public sealed record LinkUnitCommand(Guid CommunityId, Guid MainUnitId, Guid RelatedUnitId, PropertyRelationType RelationType, DateOnly StartsOn);
public sealed record CreateAliquotCommand(Guid CommunityId, Guid UnitId, AliquotCalculationMethod Method, decimal UnitComputableArea, decimal TotalComputableArea, decimal Coefficient, decimal FixedPercentage, decimal ManualValue, DateOnly ValidFrom, string Reason, string? SupportDocumentReference, string ApprovedBy);
public sealed record PropertyUnitSummary(Guid Id, string Code, string Location, decimal MainAreaM2, PropertyUnitStatus Status);
public sealed record CoefficientValidationResult(Guid CommunityId, decimal Total, decimal DifferenceFrom100, bool IsValid, int UnitCount);
public sealed record DemoGenerationResult(Guid CommunityId, int UnitsCreated, int TypesCreated, decimal AliquotTotal);
public sealed record PropertyRecordResult(PropertyUnit Unit, IReadOnlyCollection<PropertyArea> Areas, IReadOnlyCollection<AliquotVersion> Aliquots);

public sealed class PropertyService(IPropertyRepository repository)
{
    public async Task<Guid> CreateTypeAsync(CreateUnitTypeCommand command, CancellationToken cancellationToken)
    {
        if (await repository.TypeCodeExistsAsync(command.CommunityId, command.Code.Trim().ToLowerInvariant(), cancellationToken))
            throw new InvalidOperationException("Property unit type code already exists in the community.");
        var type = PropertyUnitType.Create(command.CommunityId, command.Code, command.Name);
        await repository.AddAsync(type, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return type.Id;
    }

    public async Task<Guid> CreateUnitAsync(CreateUnitCommand command, CancellationToken cancellationToken)
    {
        PropertyUnitType? type = await repository.GetTypeAsync(command.TypeId, cancellationToken);
        if (type is null || type.CommunityId != command.CommunityId || !type.IsActive)
            throw new InvalidOperationException("Active unit type was not found for this community.");
        if (await repository.UnitCodeExistsAsync(command.CommunityId, command.Code.Trim().ToLowerInvariant(), cancellationToken))
            throw new InvalidOperationException("Property unit code already exists in the community.");
        var unit = PropertyUnit.Create(command.CommunityId, command.TypeId, command.Code, command.Location, command.MainAreaM2, command.Notes);
        await repository.AddAsync(unit, cancellationToken);
        await repository.AddAsync(PropertyArea.Create(command.CommunityId, unit.Id, PropertyAreaType.Main, command.MainAreaM2, true, "Main registered area"), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return unit.Id;
    }

    public async Task<Guid> AddAreaAsync(AddAreaCommand command, CancellationToken cancellationToken)
    {
        PropertyUnit unit = await RequireUnitAsync(command.CommunityId, command.UnitId, cancellationToken);
        var area = PropertyArea.Create(unit.CommunityId, unit.Id, command.Type, command.AreaM2, command.IsComputable, command.Description);
        await repository.AddAsync(area, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return area.Id;
    }

    public async Task<Guid> LinkUnitAsync(LinkUnitCommand command, CancellationToken cancellationToken)
    {
        _ = await RequireUnitAsync(command.CommunityId, command.MainUnitId, cancellationToken);
        _ = await RequireUnitAsync(command.CommunityId, command.RelatedUnitId, cancellationToken);
        var relation = PropertyRelation.Create(command.CommunityId, command.MainUnitId, command.RelatedUnitId, command.RelationType, command.StartsOn);
        await repository.AddAsync(relation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return relation.Id;
    }

    public async Task<Guid> CreateAliquotAsync(CreateAliquotCommand command, CancellationToken cancellationToken)
    {
        _ = await RequireUnitAsync(command.CommunityId, command.UnitId, cancellationToken);
        decimal value = AliquotCalculator.Calculate(command.Method, command.UnitComputableArea, command.TotalComputableArea, command.Coefficient, command.FixedPercentage, command.ManualValue);
        var aliquot = AliquotVersion.Create(command.CommunityId, command.UnitId, command.Method, value, command.ValidFrom, command.Reason, command.SupportDocumentReference);
        aliquot.Approve(command.ApprovedBy, DateTimeOffset.UtcNow);
        await repository.AddAsync(aliquot, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return aliquot.Id;
    }

    public async Task<IReadOnlyCollection<PropertyUnitSummary>> ListUnitsAsync(Guid communityId, CancellationToken cancellationToken) =>
        (await repository.ListUnitsAsync(communityId, cancellationToken))
            .OrderBy(x => x.Code)
            .Select(x => new PropertyUnitSummary(x.Id, x.Code, x.Location, x.MainAreaM2, x.Status))
            .ToArray();

    public async Task<PropertyRecordResult?> GetRecordAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken)
    {
        PropertyUnit? unit = await repository.GetUnitAsync(unitId, cancellationToken);
        if (unit is null || unit.CommunityId != communityId) return null;
        var areas = await repository.ListAreasAsync(communityId, unitId, cancellationToken);
        var aliquots = (await repository.ListAliquotsAsync(communityId, cancellationToken)).Where(x => x.UnitId == unitId).OrderByDescending(x => x.ValidFrom).ToArray();
        return new(unit, areas, aliquots);
    }

    public async Task<CoefficientValidationResult> ValidateCoefficientsAsync(Guid communityId, CancellationToken cancellationToken)
    {
        var units = await repository.ListUnitsAsync(communityId, cancellationToken);
        var aliquots = await repository.ListAliquotsAsync(communityId, cancellationToken);
        decimal total = units.Sum(unit => aliquots.Where(x => x.UnitId == unit.Id && x.ValidTo == null && x.ApprovedAtUtc != null).OrderByDescending(x => x.ValidFrom).Select(x => x.Value).FirstOrDefault());
        decimal difference = decimal.Round(100m - total, 6, MidpointRounding.AwayFromZero);
        return new(communityId, total, difference, Math.Abs(difference) <= 0.0001m, units.Count);
    }

    public async Task<DemoGenerationResult> GenerateDemoAsync(Guid communityId, int unitCount, CancellationToken cancellationToken)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("Community is required.", nameof(communityId));
        if (unitCount is < 1 or > 5000) throw new ArgumentOutOfRangeException(nameof(unitCount));
        if ((await repository.ListUnitsAsync(communityId, cancellationToken)).Count > 0)
            throw new InvalidOperationException("Demo generation is allowed only for an empty community cadastre.");

        PropertyUnitType apartment = PropertyUnitType.Create(communityId, "apartment", "Apartment");
        PropertyUnitType parking = PropertyUnitType.Create(communityId, "parking", "Parking");
        PropertyUnitType storage = PropertyUnitType.Create(communityId, "storage", "Storage");
        await repository.AddAsync(apartment, cancellationToken);
        await repository.AddAsync(parking, cancellationToken);
        await repository.AddAsync(storage, cancellationToken);

        decimal aliquot = decimal.Round(100m / unitCount, 6, MidpointRounding.AwayFromZero);
        decimal accumulated = 0m;
        for (int i = 1; i <= unitCount; i++)
        {
            var unit = PropertyUnit.Create(communityId, apartment.Id, $"A-{i:000}", $"Tower A / Unit {i:000}", 80m + (i % 5) * 5m, "Demo data");
            await repository.AddAsync(unit, cancellationToken);
            await repository.AddAsync(PropertyArea.Create(communityId, unit.Id, PropertyAreaType.Main, unit.MainAreaM2, true, "Demo main area"), cancellationToken);
            decimal unitAliquot = i == unitCount ? decimal.Round(100m - accumulated, 6, MidpointRounding.AwayFromZero) : aliquot;
            accumulated += unitAliquot;
            var version = AliquotVersion.Create(communityId, unit.Id, AliquotCalculationMethod.FixedPercentage, unitAliquot, new DateOnly(2026, 1, 1), "Initial demo coefficient", null);
            version.Approve("demo-generator", DateTimeOffset.UtcNow);
            await repository.AddAsync(version, cancellationToken);
        }
        await repository.SaveChangesAsync(cancellationToken);
        return new(communityId, unitCount, 3, accumulated);
    }

    private async Task<PropertyUnit> RequireUnitAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken)
    {
        PropertyUnit? unit = await repository.GetUnitAsync(unitId, cancellationToken);
        return unit is null || unit.CommunityId != communityId ? throw new KeyNotFoundException("Property unit was not found in this community.") : unit;
    }
}
