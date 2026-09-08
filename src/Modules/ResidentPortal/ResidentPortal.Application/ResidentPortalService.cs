using AppCondominio.Modules.Billing.Application;
using AppCondominio.Modules.Collections.Application;
using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.Reservations.Application;
using AppCondominio.Modules.SecurityOperations.Application;

namespace AppCondominio.Modules.ResidentPortal.Application;

public sealed class ResidentPortalService(
    ResidentAccessService access,
    MonthlyBillingService billing,
    CollectionsService collections,
    ReservationService reservations,
    SecurityOperationsService security)
{
    public Task<IReadOnlyCollection<ResidentUnitAccess>> GetContextAsync(string externalUserId, CancellationToken ct) =>
        access.GetCurrentAsync(externalUserId, ct);

    public async Task<object> GetStatementAsync(string externalUserId, Guid unitId, CancellationToken ct)
    {
        var current = await access.RequireUnitAsync(externalUserId, unitId, ct);
        var statement = await billing.GetStatementAsync(current.CommunityId, current.UnitId, ct);
        var receivables = await collections.GetReceivablesAsync(current.CommunityId, current.UnitId, DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return new { Access = current, Statement = statement, Receivables = receivables };
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
