using AppCondominio.Modules.Procurement.Domain;
namespace AppCondominio.UnitTests;
public sealed class SupplierPortalAccessTests
{
 [Fact]
 public void Supplier_access_grant_enforces_validity_and_revocation()
 {
  var now=DateTimeOffset.UtcNow;
  var grant=SupplierAccessGrant.Create(Guid.NewGuid(),Guid.NewGuid(),"portal-supplier-user",now.AddMinutes(-1),now.AddHours(1));
  Assert.True(grant.IsUsable(now));
  grant.Revoke("supplier disabled");
  Assert.False(grant.IsUsable(now));
  Assert.Equal(SupplierAccessStatus.Revoked,grant.Status);
 }
 [Fact]
 public void Supplier_access_grant_expires_when_due()
 {
  var now=DateTimeOffset.UtcNow;
  var grant=SupplierAccessGrant.Create(Guid.NewGuid(),Guid.NewGuid(),"portal-supplier-expiring",now.AddHours(-2),now.AddMinutes(-1));
  Assert.True(grant.ExpireIfDue(now));
  Assert.Equal(SupplierAccessStatus.Expired,grant.Status);
  Assert.False(grant.IsUsable(now));
 }
}