using AppCondominio.Modules.People.Domain;

namespace AppCondominio.UnitTests;

public sealed class PeopleDomainTests
{
    [Fact]
    public void Access_code_is_hashed_and_matches_in_constant_time_path()
    {
        var code=UnitAccessCode.Create(Guid.NewGuid(),Guid.NewGuid(),"ABC-123",DateTimeOffset.UtcNow.AddHours(1));
        Assert.NotEqual("ABC-123",code.CodeHash);
        Assert.Equal(64,code.CodeHash.Length);
        Assert.True(code.Matches("ABC-123"));
        Assert.False(code.Matches("ABC-124"));
    }

    [Fact]
    public void Ownership_rejects_invalid_percentage()
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>Ownership.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),101m,DateOnly.FromDateTime(DateTime.UtcNow),"purchase",null));
    }

    [Fact]
    public void Lease_normalizes_permissions_and_expires_on_end_date()
    {
        var lease=LeaseContract.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),new DateOnly(2026,1,1),new DateOnly(2026,1,31),new[]{" VIEW ","pay","view"},null);
        Assert.Equal("pay,view",lease.PermissionsCsv);
        lease.Expire(new DateOnly(2026,1,31));
        Assert.False(lease.IsActive);
    }

    [Fact]
    public void Access_grant_expires_only_when_due()
    {
        var due=DateTimeOffset.UtcNow.AddMinutes(10);
        var grant=AccessGrant.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),"portal-user",OccupancyRole.Tenant,DateTimeOffset.UtcNow,due);
        Assert.False(grant.ExpireIfDue(due.AddSeconds(-1)));
        Assert.True(grant.ExpireIfDue(due));
        Assert.Equal(AccessGrantStatus.Expired,grant.Status);
    }
}
