namespace AppCondominio.Contracts.Security;

public static class AppCondominioPermissions
{
    public const string ClaimType = "permission";
    public static class Organizations{public const string Resource="appcondominio.organizations";public const string Read="appcondominio.organizations.read";public const string Manage="appcondominio.organizations.manage";}
    public static class Saas{public const string Resource="appcondominio.saas";public const string Read="appcondominio.saas.read";public const string Manage="appcondominio.saas.manage";}
    public static class Communities{public const string Resource="appcondominio.communities";public const string Read="appcondominio.communities.read";public const string Manage="appcondominio.communities.manage";}
    public static class Properties{public const string Resource="appcondominio.properties";public const string Read="appcondominio.properties.read";public const string Manage="appcondominio.properties.manage";}
    public static class People{public const string Resource="appcondominio.people";public const string Read="appcondominio.people.read";public const string Manage="appcondominio.people.manage";}
    public static class Billing{public const string Resource="appcondominio.billing";public const string Read="appcondominio.billing.read";public const string Manage="appcondominio.billing.manage";}
    public static readonly string[] All=[Organizations.Read,Organizations.Manage,Saas.Read,Saas.Manage,Communities.Read,Communities.Manage,Properties.Read,Properties.Manage,People.Read,People.Manage,Billing.Read,Billing.Manage];
}
