using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Billing.Infrastructure.Migrations;

[DbContext(typeof(BillingDbContext))]
[Migration("20260908142000_InitialBilling")]
public partial class InitialBilling:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("billing");
        m.CreateTable(name:"ChargeConcepts",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Code=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),Name=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),Status=t.Column<int>(type:"int",nullable:false)},constraints:c=>c.PrimaryKey("PK_ChargeConcepts",x=>x.Id));
        m.CreateIndex(name:"IX_ChargeConcepts_CommunityId_Code",schema:"billing",table:"ChargeConcepts",columns:new[]{"CommunityId","Code"},unique:true);

        m.CreateTable(name:"ChargeConceptVersions",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Version=t.Column<int>(type:"int",nullable:false),Method=t.Column<int>(type:"int",nullable:false),Periodicity=t.Column<int>(type:"int",nullable:false),ValidFrom=t.Column<DateOnly>(type:"date",nullable:false),ValidTo=t.Column<DateOnly>(type:"date",nullable:true),FixedAmount=t.Column<decimal>(type:"decimal(18,6)",nullable:false),Rate=t.Column<decimal>(type:"decimal(18,6)",nullable:false),Minimum=t.Column<decimal>(type:"decimal(18,6)",nullable:false),Maximum=t.Column<decimal>(type:"decimal(18,6)",nullable:false),IsApproved=t.Column<bool>(type:"bit",nullable:false),ApprovedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),ApprovedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true)},constraints:c=>c.PrimaryKey("PK_ChargeConceptVersions",x=>x.Id));
        m.CreateIndex(name:"IX_ChargeConceptVersions_CommunityId_ConceptId_Version",schema:"billing",table:"ChargeConceptVersions",columns:new[]{"CommunityId","ConceptId","Version"},unique:true);

        m.CreateTable(name:"AccountingMappings",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),RevenueAccount=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),ReceivableAccount=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),TaxAccount=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:true),CostCenter=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:true)},constraints:c=>c.PrimaryKey("PK_AccountingMappings",x=>x.Id));
        m.CreateIndex(name:"IX_AccountingMappings_CommunityId_ConceptId",schema:"billing",table:"AccountingMappings",columns:new[]{"CommunityId","ConceptId"});

        m.CreateTable(name:"InterestRules",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Percentage=t.Column<decimal>(type:"decimal(18,6)",nullable:false),FixedAmount=t.Column<decimal>(type:"decimal(18,6)",nullable:false),GraceDays=t.Column<int>(type:"int",nullable:false),EveryDays=t.Column<int>(type:"int",nullable:false),MaximumAmount=t.Column<decimal>(type:"decimal(18,6)",nullable:false)},constraints:c=>c.PrimaryKey("PK_InterestRules",x=>x.Id));
        m.CreateIndex(name:"IX_InterestRules_CommunityId_ConceptId",schema:"billing",table:"InterestRules",columns:new[]{"CommunityId","ConceptId"});

        m.CreateTable(name:"DiscountRules",schema:"billing",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ConceptId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ValidFrom=t.Column<DateOnly>(type:"date",nullable:false),ValidTo=t.Column<DateOnly>(type:"date",nullable:true),Percentage=t.Column<decimal>(type:"decimal(18,6)",nullable:false),FixedAmount=t.Column<decimal>(type:"decimal(18,6)",nullable:false),PayBeforeDueDays=t.Column<int>(type:"int",nullable:false)},constraints:c=>c.PrimaryKey("PK_DiscountRules",x=>x.Id));
        m.CreateIndex(name:"IX_DiscountRules_CommunityId_ConceptId_ValidFrom",schema:"billing",table:"DiscountRules",columns:new[]{"CommunityId","ConceptId","ValidFrom"});
    }
    protected override void Down(MigrationBuilder m){foreach(var t in new[]{"AccountingMappings","DiscountRules","InterestRules","ChargeConceptVersions","ChargeConcepts"})m.DropTable(name:t,schema:"billing");}
}
