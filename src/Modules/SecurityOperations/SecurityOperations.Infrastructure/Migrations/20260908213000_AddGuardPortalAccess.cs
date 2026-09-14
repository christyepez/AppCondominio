using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace AppCondominio.Modules.SecurityOperations.Infrastructure.Migrations;
[DbContext(typeof(SecurityOperationsDbContext))]
[Migration("20260908213000_AddGuardPortalAccess")]
public partial class AddGuardPortalAccess:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.CreateTable(name:"GuardAccessGrants",schema:"securityops",columns:t=>new
  {
   Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),
   CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),
   ExternalUserId=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),
   DisplayName=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),
   StartsAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),
   ExpiresAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),
   Status=t.Column<int>(type:"int",nullable:false),
   RevocationReason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true)
  },constraints:c=>c.PrimaryKey("PK_GuardAccessGrants",x=>x.Id));
  m.CreateIndex(name:"IX_GuardAccessGrants_ExternalUserId_Status",schema:"securityops",table:"GuardAccessGrants",columns:new[]{"ExternalUserId","Status"},unique:true,filter:"[Status] = 1");
  m.CreateIndex(name:"IX_GuardAccessGrants_CommunityId_Status",schema:"securityops",table:"GuardAccessGrants",columns:new[]{"CommunityId","Status"});
 }
 protected override void Down(MigrationBuilder m)=>m.DropTable(name:"GuardAccessGrants",schema:"securityops");
}