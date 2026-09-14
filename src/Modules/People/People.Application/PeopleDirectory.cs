using AppCondominio.Contracts.People;

namespace AppCondominio.Modules.People.Application;

public sealed class PeopleDirectory(IPeopleRepository repository) : IPeopleDirectory
{
    public async Task<bool> ExistsInCommunityAsync(
        Guid communityId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        if (communityId == Guid.Empty || personId == Guid.Empty) return false;
        var person = await repository.GetPersonAsync(personId, cancellationToken);
        return person is not null && person.CommunityId == communityId;
    }
}
