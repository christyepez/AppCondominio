using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.Tax.Infrastructure.Migrations;

[DbContext(typeof(TaxDbContext))]
[Migration("20260908154500_InitialTax")]
public partial class InitialTax:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("tax");
        m.CreateTable(name:"ElectronicDocuments",schema:"tax",columns:t=>new
        {
            Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),SourceDocumentId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),DocumentType=t.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),EstablishmentCode=t.Column<string>(type:"nvarchar(10)",maxLength:10,nullable:false),EmissionPoint=t.Column<string>(type:"nvarchar(10)",maxLength:10,nullable:false),Sequential=t.Column<string>(type:"nvarchar(20)",maxLength:20,nullable:false),AccessKey=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),PayloadReference=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:false),Status=t.Column<int>(type:"int",nullable:false),AuthorizationNumber=t.Column<string>(type:"nvarchar(100)",maxLength:100,nullable:true),AuthorizedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),LastMessage=t.Column<string>(type:"nvarchar(1000)",maxLength:1000,nullable:true),SubmittedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true)
        },constraints:c=>c.PrimaryKey("PK_ElectronicDocuments",x=>x.Id));
        m.CreateIndex(name:"IX_ElectronicDocuments_CommunityId_SourceDocumentId",schema:"tax",table:"ElectronicDocuments",columns:new[]{"CommunityId","SourceDocumentId"},unique:true);
        m.CreateIndex(name:"IX_ElectronicDocuments_AccessKey",schema:"tax",table:"ElectronicDocuments",column:"AccessKey",unique:true);
    }
    protected override void Down(MigrationBuilder m)=>m.DropTable(name:"ElectronicDocuments",schema:"tax");
}
