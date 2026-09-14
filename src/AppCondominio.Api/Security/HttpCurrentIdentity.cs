using System.Security.Claims;
using AppCondominio.Contracts.Security;

namespace AppCondominio.Api.Security;

public sealed class HttpCurrentIdentity(IHttpContextAccessor accessor) : ICurrentIdentity
{
    public const string PermissionClaimType = "permission";

    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public string? UserId =>
        Principal?.FindFirst("sub")?.Value
        ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public IReadOnlySet<string> Permissions => Principal?
        .FindAll(PermissionClaimType)
        .Select(x => x.Value)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .ToHashSet(StringComparer.Ordinal)
        ?? new HashSet<string>(StringComparer.Ordinal);

    public bool HasPermission(string permission) =>
        !string.IsNullOrWhiteSpace(permission) && Permissions.Contains(permission);
}
