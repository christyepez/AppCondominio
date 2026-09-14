using AppCondominio.Modules.Governance.Domain;

namespace AppCondominio.UnitTests;

public sealed class GovernanceTests
{
    [Fact]
    public void Assembly_requires_quorum_before_opening()
    {
        var assembly = Assembly.Create(Guid.NewGuid(), "Annual assembly", DateTimeOffset.UtcNow.AddDays(1), 60m);
        Assert.Throws<InvalidOperationException>(() => assembly.Open(100, 59));
        assembly.Open(100, 60);
        Assert.Equal(AssemblyStatus.Open, assembly.Status);
    }

    [Fact]
    public void Motion_cannot_accept_votes_after_close()
    {
        var motion = Motion.Create(Guid.NewGuid(), Guid.NewGuid(), "Approve budget");
        motion.RegisterVote(VoteChoice.Yes);
        motion.RegisterVote(VoteChoice.No);
        motion.RegisterVote(VoteChoice.Yes);
        motion.Close();
        Assert.True(motion.IsApproved);
        Assert.Throws<InvalidOperationException>(() => motion.RegisterVote(VoteChoice.Yes));
    }

    [Fact]
    public void Coexistence_penalty_keeps_only_billing_reference()
    {
        var item = CoexistenceCase.Create(Guid.NewGuid(), Guid.NewGuid(), "Noise", "Repeated loud music", "resident");
        item.StartReview();
        item.Resolve("Penalty approved", 25.50m, "governance:case:001");
        Assert.Equal(CoexistenceCaseStatus.Resolved, item.Status);
        Assert.Equal(25.50m, item.PenaltyAmount);
        Assert.Equal("governance:case:001", item.BillingReference);
    }
}
