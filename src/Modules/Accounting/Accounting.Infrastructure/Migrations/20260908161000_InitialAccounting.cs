using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Accounting.Infrastructure.Migrations;

[DbContext(typeof(AccountingDbContext))]
[Migration("20260908161000_InitialAccounting")]
public partial class InitialAccounting:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("accounting");
        m.CreateTable(name:"LedgerAccounts",schema:"accounting",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Code=t.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),Name=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),Type=t.Column<int>(type:"int",nullable:false),AllowsPosting=t.Column<bool>(type:"bit",nullable:false),IsActive=t.Column<bool>(type:"bit",nullable:false)},constraints:c=>c.PrimaryKey("PK_LedgerAccounts",x=>x.Id));
        m.CreateIndex(name:"IX_LedgerAccounts_CommunityId_Code",schema:"accounting",table:"LedgerAccounts",columns:new[]{"CommunityId","Code"},unique:true);
        m.CreateTable(name:"AccountingPeriods",schema:"accounting",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Year=t.Column<int>(type:"int",nullable:false),Month=t.Column<int>(type:"int",nullable:false),Status=t.Column<int>(type:"int",nullable:false),ClosedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ClosedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true)},constraints:c=>c.PrimaryKey("PK_AccountingPeriods",x=>x.Id));
        m.CreateIndex(name:"IX_AccountingPeriods_CommunityId_Year_Month",schema:"accounting",table:"AccountingPeriods",columns:new[]{"CommunityId","Year","Month"},unique:true);
        m.CreateTable(name:"JournalEntries",schema:"accounting",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PeriodId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),EntryDate=t.Column<DateOnly>(type:"date",nullable:false),Description=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:false),SourceType=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),SourceReference=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),Status=t.Column<int>(type:"int",nullable:false),PostedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),PostedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true)},constraints:c=>c.PrimaryKey("PK_JournalEntries",x=>x.Id));
        m.CreateIndex(name:"IX_JournalEntries_CommunityId_SourceType_SourceReference",schema:"accounting",table:"JournalEntries",columns:new[]{"CommunityId","SourceType","SourceReference"},unique:true);
        m.CreateTable(name:"JournalLines",schema:"accounting",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),JournalEntryId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),AccountId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),AccountCode=t.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),Debit=t.Column<decimal>(type:"decimal(18,2)",nullable:false),Credit=t.Column<decimal>(type:"decimal(18,2)",nullable:false),CostCenter=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:true)},constraints:c=>c.PrimaryKey("PK_JournalLines",x=>x.Id));
        m.CreateIndex(name:"IX_JournalLines_JournalEntryId",schema:"accounting",table:"JournalLines",column:"JournalEntryId");
    }
    protected override void Down(MigrationBuilder m){foreach(var t in new[]{"JournalLines","JournalEntries","AccountingPeriods","LedgerAccounts"})m.DropTable(name:t,schema:"accounting");}
}
