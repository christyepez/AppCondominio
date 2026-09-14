using AppCondominio.Modules.SecurityOperations.Domain;
namespace AppCondominio.UnitTests;
public sealed class GuardPortalAccessTests
{
 [Fact] public void Guard_access_can_be_revoked(){var now=DateTimeOffset.UtcNow;var grant=GuardAccessGrant.Create(Guid.NewGuid(),"guard-user","Guard One",now.AddMinutes(-1),now.AddHours(1));Assert.True(grant.IsUsable(now));grant.Revoke("shift ended");Assert.Equal(GuardAccessStatus.Revoked,grant.Status);Assert.False(grant.IsUsable(now));}
 [Fact] public void Guard_access_expires_when_due(){var now=DateTimeOffset.UtcNow;var grant=GuardAccessGrant.Create(Guid.NewGuid(),"guard-expiring","Guard Expiring",now.AddHours(-2),now.AddMinutes(-1));Assert.True(grant.ExpireIfDue(now));Assert.Equal(GuardAccessStatus.Expired,grant.Status);Assert.False(grant.IsUsable(now));}
}