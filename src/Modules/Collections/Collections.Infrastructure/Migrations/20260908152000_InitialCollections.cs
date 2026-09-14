using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Collections.Infrastructure.Migrations;

[DbContext(typeof(CollectionsDbContext))]
[Migration("20260908152000_InitialCollections")]
public partial class InitialCollections:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("collections");
        m.CreateTable(name:"Receivables",schema:"collections",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ResponsiblePersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),BillingObligationId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),OriginalAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),OutstandingAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),IssuedOn=t.Column<DateOnly>(type:"date",nullable:false),DueOn=t.Column<DateOnly>(type:"date",nullable:false),Status=t.Column<int>(type:"int",nullable:false)},constraints:c=>c.PrimaryKey("PK_Receivables",x=>x.Id));
        m.CreateIndex(name:"IX_Receivables_BillingObligationId",schema:"collections",table:"Receivables",column:"BillingObligationId",unique:true);
        m.CreateIndex(name:"IX_Receivables_CommunityId_UnitId_Status_DueOn",schema:"collections",table:"Receivables",columns:new[]{"CommunityId","UnitId","Status","DueOn"});

        m.CreateTable(name:"Payments",schema:"collections",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Reference=t.Column<string>(type:"nvarchar(120)",maxLength:120,nullable:false),Method=t.Column<int>(type:"int",nullable:false),ReceivedOn=t.Column<DateOnly>(type:"date",nullable:false),Amount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),UnappliedAmount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),ExternalTransactionId=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),Status=t.Column<int>(type:"int",nullable:false)},constraints:c=>c.PrimaryKey("PK_Payments",x=>x.Id));
        m.CreateIndex(name:"IX_Payments_CommunityId_Reference",schema:"collections",table:"Payments",columns:new[]{"CommunityId","Reference"},unique:true);
        m.CreateIndex(name:"IX_Payments_CommunityId_ExternalTransactionId",schema:"collections",table:"Payments",columns:new[]{"CommunityId","ExternalTransactionId"});

        m.CreateTable(name:"PaymentApplications",schema:"collections",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PaymentId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ReceivableId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Amount=t.Column<decimal>(type:"decimal(18,2)",nullable:false),AppliedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),AppliedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),ReversedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),ReversedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true)},constraints:c=>c.PrimaryKey("PK_PaymentApplications",x=>x.Id));
        m.CreateIndex(name:"IX_PaymentApplications_PaymentId_ReceivableId",schema:"collections",table:"PaymentApplications",columns:new[]{"PaymentId","ReceivableId"});
    }
    protected override void Down(MigrationBuilder m){m.DropTable(name:"PaymentApplications",schema:"collections");m.DropTable(name:"Payments",schema:"collections");m.DropTable(name:"Receivables",schema:"collections");}
}
