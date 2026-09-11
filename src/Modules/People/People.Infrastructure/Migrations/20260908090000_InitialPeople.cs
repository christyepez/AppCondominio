using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace AppCondominio.Modules.People.Infrastructure.Migrations;

[DbContext(typeof(PeopleDbContext))]
[Migration("20260908090000_InitialPeople")]
public partial class InitialPeople:Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.EnsureSchema("people");
        m.CreateTable(name:"People",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Kind=t.Column<int>(type:"int",nullable:false),Identification=t.Column<string>(type:"nvarchar(40)",maxLength:40,nullable:false),DisplayName=t.Column<string>(type:"nvarchar(250)",maxLength:250,nullable:false),Email=t.Column<string>(type:"nvarchar(320)",maxLength:320,nullable:true),Phone=t.Column<string>(type:"nvarchar(60)",maxLength:60,nullable:true),BirthDate=t.Column<DateOnly>(type:"date",nullable:true),Address=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true),LegalRepresentative=t.Column<string>(type:"nvarchar(250)",maxLength:250,nullable:true),IsActive=t.Column<bool>(type:"bit",nullable:false)},constraints:c=>c.PrimaryKey("PK_People",x=>x.Id));
        m.CreateIndex(name:"IX_People_CommunityId_Identification",schema:"people",table:"People",columns:new[]{"CommunityId","Identification"},unique:true);

        m.CreateTable(name:"Ownerships",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Percentage=t.Column<decimal>(type:"decimal(18,6)",nullable:false),StartsOn=t.Column<DateOnly>(type:"date",nullable:false),EndsOn=t.Column<DateOnly>(type:"date",nullable:true),AcquisitionType=t.Column<string>(type:"nvarchar(80)",maxLength:80,nullable:false),SupportDocumentReference=t.Column<string>(type:"nvarchar(1000)",maxLength:1000,nullable:true)},constraints:c=>c.PrimaryKey("PK_Ownerships",x=>x.Id));
        m.CreateIndex(name:"IX_Ownerships_CommunityId_UnitId_PersonId_StartsOn",schema:"people",table:"Ownerships",columns:new[]{"CommunityId","UnitId","PersonId","StartsOn"});

        m.CreateTable(name:"Leases",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),TenantPersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ResponsibleOwnerPersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),StartsOn=t.Column<DateOnly>(type:"date",nullable:false),EndsOn=t.Column<DateOnly>(type:"date",nullable:false),PermissionsCsv=t.Column<string>(type:"nvarchar(2000)",maxLength:2000,nullable:false),ContractDocumentReference=t.Column<string>(type:"nvarchar(1000)",maxLength:1000,nullable:true),IsActive=t.Column<bool>(type:"bit",nullable:false)},constraints:c=>c.PrimaryKey("PK_Leases",x=>x.Id));
        m.CreateIndex(name:"IX_Leases_CommunityId_UnitId_EndsOn",schema:"people",table:"Leases",columns:new[]{"CommunityId","UnitId","EndsOn"});

        m.CreateTable(name:"Residents",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),Role=t.Column<int>(type:"int",nullable:false),StartsOn=t.Column<DateOnly>(type:"date",nullable:false),EndsOn=t.Column<DateOnly>(type:"date",nullable:true)},constraints:c=>c.PrimaryKey("PK_Residents",x=>x.Id));
        m.CreateIndex(name:"IX_Residents_CommunityId_UnitId_PersonId_StartsOn",schema:"people",table:"Residents",columns:new[]{"CommunityId","UnitId","PersonId","StartsOn"});

        m.CreateTable(name:"AccessCodes",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CodeHash=t.Column<string>(type:"nvarchar(64)",maxLength:64,nullable:false),ExpiresAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),ConsumedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true)},constraints:c=>c.PrimaryKey("PK_AccessCodes",x=>x.Id));
        m.CreateIndex(name:"IX_AccessCodes_CommunityId_UnitId_ExpiresAtUtc",schema:"people",table:"AccessCodes",columns:new[]{"CommunityId","UnitId","ExpiresAtUtc"});

        m.CreateTable(name:"Activations",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ExternalUserId=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),RequestedRole=t.Column<int>(type:"int",nullable:false),Status=t.Column<int>(type:"int",nullable:false),RequestedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),DecidedAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),DecidedBy=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:true),DecisionReason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true)},constraints:c=>c.PrimaryKey("PK_Activations",x=>x.Id));
        m.CreateIndex(name:"IX_Activations_CommunityId_UnitId_Status",schema:"people",table:"Activations",columns:new[]{"CommunityId","UnitId","Status"});

        m.CreateTable(name:"AccessGrants",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),ExternalUserId=t.Column<string>(type:"nvarchar(200)",maxLength:200,nullable:false),Role=t.Column<int>(type:"int",nullable:false),StartsAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:false),ExpiresAtUtc=t.Column<DateTimeOffset>(type:"datetimeoffset",nullable:true),Status=t.Column<int>(type:"int",nullable:false),RevocationReason=t.Column<string>(type:"nvarchar(500)",maxLength:500,nullable:true)},constraints:c=>c.PrimaryKey("PK_AccessGrants",x=>x.Id));
        m.CreateIndex(name:"IX_AccessGrants_CommunityId_ExternalUserId_Status",schema:"people",table:"AccessGrants",columns:new[]{"CommunityId","ExternalUserId","Status"});

        m.CreateTable(name:"FinancialResponsibilities",schema:"people",columns:t=>new{Id=t.Column<Guid>(type:"uniqueidentifier",nullable:false),CommunityId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),UnitId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),PersonId=t.Column<Guid>(type:"uniqueidentifier",nullable:false),StartsOn=t.Column<DateOnly>(type:"date",nullable:false),EndsOn=t.Column<DateOnly>(type:"date",nullable:true)},constraints:c=>c.PrimaryKey("PK_FinancialResponsibilities",x=>x.Id));
        m.CreateIndex(name:"IX_FinancialResponsibilities_CommunityId_UnitId_StartsOn",schema:"people",table:"FinancialResponsibilities",columns:new[]{"CommunityId","UnitId","StartsOn"});
    }

    protected override void Down(MigrationBuilder m)
    {
        foreach(var table in new[]{"AccessGrants","Activations","AccessCodes","FinancialResponsibilities","Leases","Ownerships","Residents","People"})m.DropTable(name:table,schema:"people");
    }
}
