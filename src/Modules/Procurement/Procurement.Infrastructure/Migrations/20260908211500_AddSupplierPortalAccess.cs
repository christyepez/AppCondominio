using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace AppCondominio.Modules.Procurement.Infrastructure.Migrations;
[DbContext(typeof(ProcurementDbContext))]
[Migration("20260908211500_AddSupplierPortalAccess")]
public partial class AddSupplierPortalAccess:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"SupplierAccessGrants",schema:"procurement",columns:t=>new
  {
   Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),
   CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),
   SupplierId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),
   ExternalUserId=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),
   StartsAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),
   ExpiresAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),
   Status=t.Column<int>(type:"int",nullable:false),
   RevocationReason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true)
  },constraints:c=>c.PrimaryKey("PK_SupplierAccessGrants",x=>x.Id));
  m.CreateIndex(name:"IX_SupplierAccessGrants_ExternalUserId_Status",schema:"procurement",table:"SupplierAccessGrants",columns:new[]{"ExternalUserId","Status"});
  m.CreateIndex(name:"IX_SupplierAccessGrants_SupplierId_ExternalUserId_Status",schema:"procurement",table:"SupplierAccessGrants",columns:new[]{"SupplierId","ExternalUserId","Status"});
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable(name:"SupplierAccessGrants",schema:"procurement");
}