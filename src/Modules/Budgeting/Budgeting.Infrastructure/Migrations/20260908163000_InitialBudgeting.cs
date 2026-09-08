using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Budgeting.Infrastructure.Migrations;

public partial class InitialBudgeting:Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name:"budgeting");
        migrationBuilder.CreateTable(name:"BudgetPlans",schema:"budgeting",columns:table=>new{Id=table.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=table.Column<Guid>(type:"uniqueidentifier",nullable:false),Year=table.Column<int>(type:"int",nullable:false),Version=table.Column<int>(type:"int",nullable:false),Name=table.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),Status=table.Column<int>(type:"int",nullable:false),CreatedAtUtc=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),ApprovedBy=table.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),ApprovedAtUtc=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ClosedBy=table.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),ClosedAtUtc=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true)},constraints:table=>table.PrimaryKey("PK_BudgetPlans",x=>x.Id));
        migrationBuilder.CreateTable(name:"BudgetLines",schema:"budgeting",columns:table=>new{Id=table.Column<Guid>(type:"uniqueidentifier",nullable:false),BudgetPlanId=table.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=table.Column<Guid>(type:"uniqueidentifier",nullable:false),AccountCode=table.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),Category=table.Column<string>(type:"nvarchar(120)",maxLength:120,nullable:false),Month=table.Column<int>(type:"int",nullable:false),PlannedAmount=table.Column<decimal>(type:"decimal(18,2)",nullable:false)},constraints:table=>table.PrimaryKey("PK_BudgetLines",x=>x.Id));
        migrationBuilder.CreateTable(name:"BudgetActuals",schema:"budgeting",columns:table=>new{Id=table.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=table.Column<Guid>(type:"uniqueidentifier",nullable:false),Year=table.Column<int>(type:"int",nullable:false),Month=table.Column<int>(type:"int",nullable:false),AccountCode=table.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),Amount=table.Column<decimal>(type:"decimal(18,2)",nullable:false),SourceType=table.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),SourceReference=table.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),ImportedAtUtc=table.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false)},constraints:table=>table.PrimaryKey("PK_BudgetActuals",x=>x.Id));
        migrationBuilder.CreateIndex(name:"IX_BudgetPlans_CommunityId_Year_Version",schema:"budgeting",table:"BudgetPlans",columns:new[]{"CommunityId","Year","Version"},unique:true);
        migrationBuilder.CreateIndex(name:"IX_BudgetLines_BudgetPlanId_AccountCode_Month",schema:"budgeting",table:"BudgetLines",columns:new[]{"BudgetPlanId","AccountCode","Month"},unique:true);
        migrationBuilder.CreateIndex(name:"IX_BudgetActuals_CommunityId_SourceType_SourceReference",schema:"budgeting",table:"BudgetActuals",columns:new[]{"CommunityId","SourceType","SourceReference"},unique:true);
    }
    protected override void Down(MigrationBuilder migrationBuilder){migrationBuilder.DropTable(name:"BudgetActuals",schema:"budgeting");migrationBuilder.DropTable(name:"BudgetLines",schema:"budgeting");migrationBuilder.DropTable(name:"BudgetPlans",schema:"budgeting");}
}
