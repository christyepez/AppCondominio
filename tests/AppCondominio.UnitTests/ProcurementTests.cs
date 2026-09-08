using AppCondominio.Modules.Procurement.Domain;
namespace AppCondominio.UnitTests;
public sealed class ProcurementTests
{
 [Fact] public void Requisition_requires_approval_before_sourcing(){var r=PurchaseRequisition.Create(Guid.NewGuid(),"REQ-1","Pump",1000,"user");Assert.Throws<InvalidOperationException>(()=>r.StartSourcing());r.Approve("manager");r.StartSourcing();Assert.Equal(RequisitionStatus.Sourcing,r.Status);}
 [Fact] public void Bid_score_must_be_between_zero_and_one_hundred(){var b=SupplierBid.Create(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),100,3,"ref");Assert.Throws<ArgumentOutOfRangeException>(()=>b.Score(101));b.Score(90);Assert.Equal(90,b.EvaluationScore);}
 [Fact] public void Round_must_be_closed_before_award(){var r=SourcingRound.Create(Guid.NewGuid(),Guid.NewGuid(),"Round",DateTimeOffset.UtcNow.AddDays(1));Assert.Throws<InvalidOperationException>(()=>r.Award(Guid.NewGuid()));r.Close();r.Award(Guid.NewGuid());Assert.Equal(SourcingRoundStatus.Awarded,r.Status);}
}
