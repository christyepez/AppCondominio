using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Collections.Application;
using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.Properties.Application;
using AppCondominio.Modules.Reservations.Application;
using AppCondominio.Modules.SecurityOperations.Application;

namespace AppCondominio.Modules.ResidentPortal.Application;

public sealed record ResidentHomeUnit(
    Guid CommunityId,
    Guid UnitId,
    Guid PersonId,
    string DisplayName,
    string UnitCode,
    string Location,
    decimal MainAreaM2,
    int Role,
    DateTimeOffset? AccessExpiresAtUtc);

public sealed class ResidentPortalService(
    ResidentAccessService access,
    PropertyService properties,
    MonthlyBillingService billing,
    CollectionsService collections,
    ReservationService reservations,
    SecurityOperationsService security)
{
    public Task<IReadOnlyCollection<ResidentUnitAccess>> GetContextAsync(string externalUserId, CancellationToken ct) =>
        access.GetCurrentAsync(externalUserId, ct);

    public async Task<IReadOnlyCollection<ResidentHomeUnit>> GetHomeAsync(string externalUserId, CancellationToken ct)
    {
        var grants = await access.GetCurrentAsync(externalUserId, ct);
        var result = new List<ResidentHomeUnit>(grants.Count);
        foreach (var grant in grants)
        {
            var record = await properties.GetRecordAsync(grant.CommunityId, grant.UnitId, ct);
            if (record is null) continue;
            result.Add(new(grant.CommunityId, grant.UnitId, grant.PersonId, grant.DisplayName,
                record.Unit.Code, record.Unit.Location, record.Unit.MainAreaM2, (int)grant.Role, grant.ExpiresAtUtc));
        }
        return result.OrderBy(x => x.UnitCode).ToArray();
    }

    public async Task<object> GetStatementAsync(string externalUserId, Guid unitId, CancellationToken ct)
    {
        var current = await access.RequireUnitAsync(externalUserId, unitId, ct);
        var statement = await billing.GetStatementAsync(current.CommunityId, current.UnitId, ct);
        var receivables = await collections.GetReceivablesAsync(current.CommunityId, current.UnitId, DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return new { Access = current, Statement = statement, Receivables = receivables };
    }

    public async Task<IReadOnlyCollection<ReservableAreaSummary>> GetReservableAreasAsync(string externalUserId, Guid unitId, CancellationToken ct)
    {
        var current = await access.RequireUnitAsync(externalUserId, unitId, ct);
        return await reservations.ListActiveAreasAsync(current.CommunityId, ct);
    }

    public async Task<Guid> RequestReservationAsync(string externalUserId, Guid unitId, Guid areaId, DateTimeOffset startsAt, DateTimeOffset endsAt, int guests, CancellationToken ct)
    {
        var current = await access.RequireUnitAsync(externalUserId, unitId, ct);
        return await reservations.RequestForCommunityAsync(current.CommunityId, areaId, current.PersonId, startsAt, endsAt, guests, ct);
    }

    public async Task<Guid> AuthorizeVisitAsync(string externalUserId, Guid unitId, string visitorName, string document, string destination, DateTimeOffset validFrom, DateTimeOffset validTo, CancellationToken ct)
    {
        var current = await access.RequireUnitAsync(externalUserId, unitId, ct);
        return await security.AuthorizeVisitAsync(current.CommunityId, current.PersonId, visitorName, document, destination, validFrom, validTo, ct);
    }
}
