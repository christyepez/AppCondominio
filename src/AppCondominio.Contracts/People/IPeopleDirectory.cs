namespace AppCondominio.Contracts.People;

public interface IPeopleDirectory
{
    Task<bool> ExistsInCommunityAsync(
        Guid communityId,
        Guid personId,
        CancellationToken cancellationToken = default);
}
