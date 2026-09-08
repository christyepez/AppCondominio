using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Billing.Infrastructure.Migrations;

[DbContext(typeof(BillingDbContext))]
[Migration("20260908150000_MonthlyChargeGeneration")]
public partial class MonthlyChargeGeneration:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable(name:"BillingPeriods",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Year=t.Column<int>(type:"int",nullable:false),Month=t.Column<int>(type:"int",nullable:false),IssueDate=t.Column<DateOnly>(type:"date",nullable:false),DueDate=t.Column<DateOnly>(type:"date",nullable:false),Status=t.Column<int>(type:"int",nullable:false),DraftLineCount=t.Column<int>(type:"int",nullable:false),DraftTotal=t.Column<decimal>(type:"decimal(18,2)",nullable:false),ApprovedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ApprovedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),IssuedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ReversedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ReversalReason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true)},constraints:c=>c.PrimaryKey("PK_BillingPeriods",x=>x.Id));
        m.CreateIndex(name:"IX_BillingPeriods_CommunityId_Year_Month",schema:"billing",table:"BillingPeriods",columns:new[]{"CommunityId","Year","Month"},unique:true);

        m.CreateTable(name:"DraftCharges",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PeriodId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ResponsiblePersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptVersionId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Amount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),CalculationTrace=t.Column<string>(type:"nvarchar(1000)",maxLength:1000,nullable:false)},constraints:c=>c.PrimaryKey("PK_DraftCharges",x=>x.Id));
        m.CreateIndex(name:"IX_DraftCharges_PeriodId_UnitId_ConceptId",schema:"billing",table:"DraftCharges",columns:new[]{"PeriodId","UnitId","ConceptId"},unique:true);

        m.CreateTable(name:"GenerationIssues",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PeriodId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:true),Code=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),Message=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:false)},constraints:c=>c.PrimaryKey("PK_GenerationIssues",x=>x.Id));
        m.CreateIndex(name:"IX_GenerationIssues_PeriodId",schema:"billing",table:"GenerationIssues",column:"PeriodId");

        m.CreateTable(name:"ChargeObligations",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PeriodId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),DraftChargeId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ResponsiblePersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),OriginalAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),OutstandingAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),IssuedOn=t.Column<DateOnly>(type:"date",nullable:false),DueOn=t.Column<DateOnly>(type:"date",nullable:false),Status=t.Column<int>(type:"int",nullable:false)},constraints:c=>c.PrimaryKey("PK_ChargeObligations",x=>x.Id));
        m.CreateIndex(name:"IX_ChargeObligations_DraftChargeId",schema:"billing",table:"ChargeObligations",column:"DraftChargeId",unique:true);
        m.CreateIndex(name:"IX_ChargeObligations_CommunityId_UnitId_Status",schema:"billing",table:"ChargeObligations",columns:new[]{"CommunityId","UnitId","Status"});
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DropTable(name:"ChargeObligations",schema:"billing");
        m.DropTable(name:"GenerationIssues",schema:"billing");
        m.DropTable(name:"DraftCharges",schema:"billing");
        m.DropTable(name:"BillingPeriods",schema:"billing");
    }
}
