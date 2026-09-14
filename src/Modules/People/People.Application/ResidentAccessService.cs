using AppCondominio.Modules.People.Domain;

namespace AppCondominio.Modules.People.Application;

public interface IResidentAccessRepository
{
    Task<IReadOnlyCollection<AccessGrant>> ListActiveByExternalUserAsync(string externalUserId, CancellationToken cancellationToken);
    Task<Person?> GetPersonAsync(Guid personId, CancellationToken cancellationToken);
}

public sealed record ResidentUnitAccess(
    Guid CommunityId,
    Guid UnitId,
    Guid PersonId,
    string DisplayName,
    OccupancyRole Role,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset? ExpiresAtUtc);

public sealed class ResidentAccessService(IResidentAccessRepository repository)
{
    public async Task<IReadOnlyCollection<ResidentUnitAccess>> GetCurrentAsync(string externalUserId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(externalUserId)) throw new UnauthorizedAccessException("Authenticated Portal user is required.");
        var grants = await repository.ListActiveByExternalUserAsync(externalUserId.Trim(), ct);
        var result = new List<ResidentUnitAccess>(grants.Count);
        foreach (var grant in grants.Where(x => x.Status == AccessGrantStatus.Active))
        {
            if (grant.ExpiresAtUtc is not null && grant.ExpiresAtUtc <= DateTimeOffset.UtcNow) continue;
            var person = await repository.GetPersonAsync(grant.PersonId, ct);
            if (person is null || !person.IsActive || person.CommunityId != grant.CommunityId) continue;
            result.Add(new(grant.CommunityId, grant.UnitId, grant.PersonId, person.DisplayName, grant.Role, grant.StartsAtUtc, grant.ExpiresAtUtc));
        }
        return result.OrderBy(x => x.CommunityId).ThenBy(x => x.UnitId).ToArray();
    }

    public async Task<ResidentUnitAccess> RequireUnitAsync(string externalUserId, Guid unitId, CancellationToken ct)
    {
        if (unitId == Guid.Empty) throw new ArgumentException("Unit is required.", nameof(unitId));
        var access = await GetCurrentAsync(externalUserId, ct);
        return access.FirstOrDefault(x => x.UnitId == unitId)
            ?? throw new UnauthorizedAccessException("The authenticated user does not have active access to this unit.");
    }
}
