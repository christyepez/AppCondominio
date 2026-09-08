using AppCondominio.Modules.People.Domain;

namespace AppCondominio.Modules.People.Application;

public interface IPeopleRepository
{
    Task AddAsync<T>(T entity, CancellationToken cancellationToken) where T : class;
    Task<Person?> GetPersonAsync(Guid id, CancellationToken cancellationToken);
    Task<ActivationRequest?> GetActivationAsync(Guid id, CancellationToken cancellationToken);
    Task<UnitAccessCode?> FindUsableAccessCodeAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Ownership>> ListOwnershipsAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Resident>> ListResidentsAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LeaseContract>> ListLeasesAsync(Guid communityId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccessGrant>> ListActiveGrantsAsync(Guid communityId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<FinancialResponsibility>> ListResponsibilitiesAsync(Guid communityId, Guid unitId, CancellationToken cancellationToken);
    Task<bool> IdentificationExistsAsync(Guid communityId, string identification, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record UnitPeopleRecord(Guid UnitId, IReadOnlyCollection<Ownership> Ownerships, IReadOnlyCollection<Resident> Residents, IReadOnlyCollection<FinancialResponsibility> FinancialResponsibilities);
public sealed record AccessExpiryResult(int LeasesExpired, int GrantsExpired, IReadOnlyCollection<string> ExternalUsersPendingPortalRevocation);

public sealed class PeopleService(IPeopleRepository repository)
{
    public async Task<Guid> CreateNaturalPersonAsync(Guid communityId, string identification, string names, string? email, string? phone, DateOnly? birthDate, string? address, CancellationToken ct)
    {
        await EnsureUniqueIdentification(communityId, identification, ct);
        var person = Person.Natural(communityId, identification, names, email, phone, birthDate, address);
        await repository.AddAsync(person, ct); await repository.SaveChangesAsync(ct); return person.Id;
    }

    public async Task<Guid> CreateLegalPersonAsync(Guid communityId, string taxId, string legalName, string representative, string? email, string? phone, string? address, CancellationToken ct)
    {
        await EnsureUniqueIdentification(communityId, taxId, ct);
        var person = Person.Legal(communityId, taxId, legalName, representative, email, phone, address);
        await repository.AddAsync(person, ct); await repository.SaveChangesAsync(ct); return person.Id;
    }

    public async Task<Guid> AddOwnershipAsync(Guid communityId, Guid unitId, Guid personId, decimal percentage, DateOnly startsOn, string acquisitionType, string? documentReference, CancellationToken ct)
    {
        Person person = await RequirePerson(communityId, personId, ct);
        var existing = await repository.ListOwnershipsAsync(communityId, unitId, ct);
        decimal activeTotal = existing.Where(x => x.EndsOn is null).Sum(x => x.Percentage);
        if (activeTotal + percentage > 100.0001m) throw new InvalidOperationException("Active ownership percentages cannot exceed 100%.");
        var ownership = Ownership.Create(communityId, unitId, person.Id, percentage, startsOn, acquisitionType, documentReference);
        await repository.AddAsync(ownership, ct); await repository.SaveChangesAsync(ct); return ownership.Id;
    }

    public async Task<Guid> CreateLeaseAsync(Guid communityId, Guid unitId, Guid tenantPersonId, Guid ownerPersonId, DateOnly startsOn, DateOnly endsOn, IEnumerable<string> permissions, string? documentReference, CancellationToken ct)
    {
        _ = await RequirePerson(communityId, tenantPersonId, ct); _ = await RequirePerson(communityId, ownerPersonId, ct);
        var lease = LeaseContract.Create(communityId, unitId, tenantPersonId, ownerPersonId, startsOn, endsOn, permissions, documentReference);
        await repository.AddAsync(lease, ct); await repository.SaveChangesAsync(ct); return lease.Id;
    }

    public async Task<Guid> AddResidentAsync(Guid communityId, Guid unitId, Guid personId, OccupancyRole role, DateOnly startsOn, CancellationToken ct)
    {
        _ = await RequirePerson(communityId, personId, ct);
        var resident = Resident.Create(communityId, unitId, personId, role, startsOn);
        await repository.AddAsync(resident, ct); await repository.SaveChangesAsync(ct); return resident.Id;
    }

    public async Task<Guid> CreateAccessCodeAsync(Guid communityId, Guid unitId, string rawCode, DateTimeOffset expiresAtUtc, CancellationToken ct)
    {
        var code = UnitAccessCode.Create(communityId, unitId, rawCode, expiresAtUtc);
        await repository.AddAsync(code, ct); await repository.SaveChangesAsync(ct); return code.Id;
    }

    public async Task<Guid> RequestActivationAsync(Guid communityId, Guid unitId, Guid personId, string externalUserId, OccupancyRole requestedRole, string rawCode, CancellationToken ct)
    {
        _ = await RequirePerson(communityId, personId, ct);
        UnitAccessCode? code = await repository.FindUsableAccessCodeAsync(communityId, unitId, ct);
        if (code is null || !code.IsUsable(DateTimeOffset.UtcNow) || !code.Matches(rawCode)) throw new InvalidOperationException("Activation code is invalid or expired.");
        code.Consume(DateTimeOffset.UtcNow);
        var request = ActivationRequest.Create(communityId, unitId, personId, externalUserId, requestedRole);
        await repository.AddAsync(request, ct); await repository.SaveChangesAsync(ct); return request.Id;
    }

    public async Task<Guid> ApproveActivationAsync(Guid requestId, string decidedBy, DateTimeOffset? expiresAtUtc, CancellationToken ct)
    {
        ActivationRequest request = await repository.GetActivationAsync(requestId, ct) ?? throw new KeyNotFoundException("Activation request was not found.");
        request.Approve(decidedBy);
        var grant = AccessGrant.Create(request.CommunityId, request.UnitId, request.PersonId, request.ExternalUserId, request.RequestedRole, DateTimeOffset.UtcNow, expiresAtUtc);
        await repository.AddAsync(grant, ct); await repository.SaveChangesAsync(ct); return grant.Id;
    }

    public async Task RejectActivationAsync(Guid requestId, string decidedBy, string reason, CancellationToken ct)
    {
        ActivationRequest request = await repository.GetActivationAsync(requestId, ct) ?? throw new KeyNotFoundException("Activation request was not found.");
        request.Reject(decidedBy, reason); await repository.SaveChangesAsync(ct);
    }

    public async Task SetFinancialResponsibilityAsync(Guid communityId, Guid unitId, Guid personId, DateOnly startsOn, CancellationToken ct)
    {
        _ = await RequirePerson(communityId, personId, ct);
        var current = await repository.ListResponsibilitiesAsync(communityId, unitId, ct);
        foreach (FinancialResponsibility active in current.Where(x => x.EndsOn is null)) active.End(startsOn);
        await repository.AddAsync(FinancialResponsibility.Create(communityId, unitId, personId, startsOn), ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<UnitPeopleRecord> GetUnitRecordAsync(Guid communityId, Guid unitId, CancellationToken ct) => new(
        unitId,
        await repository.ListOwnershipsAsync(communityId, unitId, ct),
        await repository.ListResidentsAsync(communityId, unitId, ct),
        await repository.ListResponsibilitiesAsync(communityId, unitId, ct));

    public async Task<AccessExpiryResult> ExpireTenantAccessAsync(Guid communityId, DateTimeOffset now, CancellationToken ct)
    {
        int leasesExpired = 0;
        foreach (LeaseContract lease in await repository.ListLeasesAsync(communityId, ct))
        {
            if (lease.IsActive && now.Date >= lease.EndsOn.ToDateTime(TimeOnly.MinValue).Date) { lease.Expire(DateOnly.FromDateTime(now.Date)); leasesExpired++; }
        }
        var pendingPortalRevocations = new List<string>();
        int grantsExpired = 0;
        foreach (AccessGrant grant in await repository.ListActiveGrantsAsync(communityId, ct))
        {
            if (grant.ExpireIfDue(now)) { grantsExpired++; pendingPortalRevocations.Add(grant.ExternalUserId); }
        }
        await repository.SaveChangesAsync(ct);
        return new(leasesExpired, grantsExpired, pendingPortalRevocations.Distinct(StringComparer.Ordinal).ToArray());
    }

    private async Task<Person> RequirePerson(Guid communityId, Guid personId, CancellationToken ct)
    {
        Person? person = await repository.GetPersonAsync(personId, ct);
        return person is null || person.CommunityId != communityId ? throw new KeyNotFoundException("Person was not found in this community.") : person;
    }
    private async Task EnsureUniqueIdentification(Guid communityId, string identification, CancellationToken ct)
    {
        if (await repository.IdentificationExistsAsync(communityId, identification.Trim(), ct)) throw new InvalidOperationException("Identification is already registered in this community.");
    }
}
