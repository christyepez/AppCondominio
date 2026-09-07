namespace AppCondominio.Api.Security;

public static class AppCondominioPermissions
{
    public const string ClaimType = "permission";

    public static class Organizations
    {
        public const string Resource = "appcondominio.organizations";
        public const string Read = "appcondominio.organizations.read";
        public const string Manage = "appcondominio.organizations.manage";
    }

    public static readonly string[] All =
    [
        Organizations.Read,
        Organizations.Manage
    ];
}
