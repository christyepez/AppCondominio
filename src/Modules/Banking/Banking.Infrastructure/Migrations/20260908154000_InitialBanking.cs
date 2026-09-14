using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Banking.Infrastructure.Migrations;

[DbContext(typeof(BankingDbContext))]
[Migration("20260908154000_InitialBanking")]
public partial class InitialBanking:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("banking");
        m.CreateTable(name:"BankAccounts",schema:"banking",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),BankCode=t.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),AccountNumber=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),Currency=t.Column<string>(type:"nvarchar(3)",maxLength:3,nullable:false),IsActive=t.Column<bool>(type:"bit",nullable:false)},constraints:c=>c.PrimaryKey("PK_BankAccounts",x=>x.Id));
        m.CreateIndex(name:"IX_BankAccounts_CommunityId_BankCode_AccountNumber",schema:"banking",table:"BankAccounts",columns:new[]{"CommunityId","BankCode","AccountNumber"},unique:true);
        m.CreateTable(name:"BankTransactions",schema:"banking",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),BankAccountId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ImportKey=t.Column<string>(type:"nvarchar(160)",maxLength:160,nullable:false),BookingDate=t.Column<DateOnly>(type:"date",nullable:false),Amount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),Type=t.Column<int>(type:"int",nullable:false),Reference=t.Column<string>(type:"nvarchar(300)",maxLength:300,nullable:false),Description=t.Column<string>(type:"nvarchar(1000)",maxLength:1000,nullable:false)},constraints:c=>c.PrimaryKey("PK_BankTransactions",x=>x.Id));
        m.CreateIndex(name:"IX_BankTransactions_BankAccountId_ImportKey",schema:"banking",table:"BankTransactions",columns:new[]{"BankAccountId","ImportKey"},unique:true);
        m.CreateIndex(name:"IX_BankTransactions_CommunityId_BookingDate_Type",schema:"banking",table:"BankTransactions",columns:new[]{"CommunityId","BookingDate","Type"});
        m.CreateTable(name:"Reconciliations",schema:"banking",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),BankTransactionId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PaymentId=t.Column<Guid>(type:"uniqueidentifier",nullable:true),Status=t.Column<int>(type:"int",nullable:false),Confidence=t.Column<decimal>(type:"decimal(5,4)",nullable:false),Reason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true),DecidedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),DecidedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true)},constraints:c=>c.PrimaryKey("PK_Reconciliations",x=>x.Id));
        m.CreateIndex(name:"IX_Reconciliations_BankTransactionId",schema:"banking",table:"Reconciliations",column:"BankTransactionId",unique:true);
        m.CreateIndex(name:"IX_Reconciliations_CommunityId_Status",schema:"banking",table:"Reconciliations",columns:new[]{"CommunityId","Status"});
    }
    protected override void Down(MigrationBuilder m){m.DropTable(name:"Reconciliations",schema:"banking");m.DropTable(name:"BankTransactions",schema:"banking");m.DropTable(name:"BankAccounts",schema:"banking");}
}
