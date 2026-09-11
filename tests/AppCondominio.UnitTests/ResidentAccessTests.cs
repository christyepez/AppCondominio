using AppCondominio.Modules.People.Application;
using AppCondominio.Modules.People.Domain;

namespace AppCondominio.UnitTests;

public sealed class ResidentAccessTests
{
    [Fact]
    public async Task Current_context_returns_only_active_grants_for_authenticated_user()
    {
        var communityId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var person = Person.Natural(communityId, "0102030405", "Resident One", null, null, null, null);
        var grant = AccessGrant.Create(communityId, unitId, person.Id, "portal-user-1", OccupancyRole.ResidentOwner, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
        var repo = new FakeRepository([grant], person);
        var service = new ResidentAccessService(repo);

        var result = await service.GetCurrentAsync("portal-user-1", CancellationToken.None);

        var access = Assert.Single(result);
        Assert.Equal(unitId, access.UnitId);
        Assert.Equal(person.Id, access.PersonId);
        Assert.Equal("Resident One", access.DisplayName);
    }

    [Fact]
    public async Task Require_unit_rejects_unit_not_granted_to_authenticated_user()
    {
        var communityId = Guid.NewGuid();
        var person = Person.Natural(communityId, "0102030406", "Resident Two", null, null, null, null);
        var grant = AccessGrant.Create(communityId, Guid.NewGuid(), person.Id, "portal-user-2", OccupancyRole.Tenant, DateTimeOffset.UtcNow.AddDays(-1), null);
        var service = new ResidentAccessService(new FakeRepository([grant], person));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RequireUnitAsync("portal-user-2", Guid.NewGuid(), CancellationToken.None));
    }

    private sealed class FakeRepository(IReadOnlyCollection<AccessGrant> grants, Person person) : IResidentAccessRepository
    {
        public Task<IReadOnlyCollection<AccessGrant>> ListActiveByExternalUserAsync(string externalUserId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<AccessGrant>>(grants.Where(x => x.ExternalUserId == externalUserId).ToArray());

        public Task<Person?> GetPersonAsync(Guid personId, CancellationToken cancellationToken) =>
            Task.FromResult<Person?>(person.Id == personId ? person : null);
    }
}
