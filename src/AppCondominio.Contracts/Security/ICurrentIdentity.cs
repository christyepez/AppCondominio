namespace AppCondominio.Contracts.Security;

public interface ICurrentIdentity
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    IReadOnlySet<string> Permissions { get; }
    bool HasPermission(string permission);
}
