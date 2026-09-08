using AppCondominio.Modules.Maintenance.Domain;
namespace AppCondominio.UnitTests;
public sealed class MaintenanceDomainTests
{
 [Fact] public void Work_order_requires_assignment_before_start(){var x=WorkOrder.Create(Guid.NewGuid(),null,null,null,"WO-1","Pump repair");Assert.Throws<InvalidOperationException>(()=>x.Start());}
 [Fact] public void Completed_work_order_records_cost_and_reference(){var x=WorkOrder.Create(Guid.NewGuid(),null,null,null,"WO-2","Generator service");x.Assign("tech");x.Start();x.Complete(125.55m,"PO-100");Assert.Equal(WorkOrderStatus.Completed,x.Status);Assert.Equal(125.55m,x.ActualCost);Assert.Equal("PO-100",x.ProcurementReference);}
 [Fact] public void Preventive_plan_moves_next_due_date(){var x=PreventivePlan.Create(Guid.NewGuid(),Guid.NewGuid(),"Quarterly",90,new DateOnly(2026,9,1));x.MarkExecuted(new DateOnly(2026,9,8));Assert.Equal(new DateOnly(2026,12,7),x.NextDueDate);}
}