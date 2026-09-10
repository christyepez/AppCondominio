using AppCondominio.Modules.Treasury.Domain;
namespace AppCondominio.UnitTests;
public sealed class TreasuryDomainTests
{
 [Fact] public void Payable_rejects_overpayment(){var p=SupplierPayable.Create(Guid.NewGuid(),Guid.NewGuid(),"INV-1",new DateOnly(2026,9,1),new DateOnly(2026,9,10),100m,"5101","2101");Assert.Throws<InvalidOperationException>(()=>p.ApplyPayment(101m));}
 [Fact] public void Request_must_be_approved_before_paid(){var r=PaymentRequest.Create(Guid.NewGuid(),Guid.NewGuid(),50m,"admin");Assert.Throws<InvalidOperationException>(()=>r.MarkPaid());r.Approve("manager");r.MarkPaid();Assert.Equal(PaymentRequestStatus.Paid,r.Status);}
 [Fact] public void Reversal_restores_payable_balance(){var p=SupplierPayable.Create(Guid.NewGuid(),Guid.NewGuid(),"INV-2",new DateOnly(2026,9,1),new DateOnly(2026,9,10),100m,"5101","2101");p.ApplyPayment(40m);p.RestorePayment(40m);Assert.Equal(100m,p.OutstandingAmount);Assert.Equal(PayableStatus.Open,p.Status);}
}
