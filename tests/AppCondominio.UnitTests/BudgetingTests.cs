using AppCondominio.Modules.Budgeting.Domain;

namespace AppCondominio.UnitTests;

public sealed class BudgetingTests
{
    [Fact] public void Budget_plan_can_be_approved_and_closed(){var p=BudgetPlan.Create(Guid.NewGuid(),2026,1,"Operating budget");p.Approve("admin");Assert.Equal(BudgetStatus.Approved,p.Status);p.Close("admin");Assert.Equal(BudgetStatus.Closed,p.Status);}
    [Fact] public void Invalid_budget_line_month_is_rejected()=>Assert.Throws<ArgumentException>(()=>BudgetLine.Create(Guid.NewGuid(),Guid.NewGuid(),"5101","Maintenance",13,100));
    [Fact] public void Negative_planned_amount_is_rejected()=>Assert.Throws<ArgumentException>(()=>BudgetLine.Create(Guid.NewGuid(),Guid.NewGuid(),"5101","Maintenance",1,-1));
    [Fact] public void Actual_keeps_source_reference_for_idempotency(){var a=BudgetActual.Create(Guid.NewGuid(),2026,9,"5101",100,"accounting","journal-1");Assert.Equal("journal-1",a.SourceReference);}
}
