namespace AppCondominio.Contracts.Security;

public static class AppCondominioPermissions
{
    public const string ClaimType = "permission";

    public static class Organizations
    {
        public const string Resource = "appcondominio.organizations";
        public const string Read = "appcondominio.organizations.read";
        public const string Manage = "appcondominio.organizations.manage";
    }

    public static class Saas
    {
        public const string Resource = "appcondominio.saas";
        public const string Read = "appcondominio.saas.read";
        public const string Manage = "appcondominio.saas.manage";
    }

    public static readonly string[] All =
    [
        Organizations.Read,
        Organizations.Manage,
        Saas.Read,
        Saas.Manage
    ];
}
