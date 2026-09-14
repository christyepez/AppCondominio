using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Communities.Infrastructure.Migrations;

[DbContext(typeof(CommunitiesDbContext))]
[Migration("20260908023000_InitialCommunities")]
public partial class InitialCommunities : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "communities");

        migrationBuilder.CreateTable(
            name: "Communities",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CommunityType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                AdministratorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                PresidentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Communities", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Communities_OrganizationId_Code",
            schema: "communities",
            table: "Communities",
            columns: new[] { "OrganizationId", "Code" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "TaxSettings",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LegalName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Establishment = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                EmissionPoint = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                AccountingObligation = table.Column<bool>(type: "bit", nullable: false),
                SriEnvironment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_TaxSettings", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_TaxSettings_CommunityId",
            schema: "communities",
            table: "TaxSettings",
            column: "CommunityId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "StructureNodes",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Type = table.Column<int>(type: "int", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_StructureNodes", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_StructureNodes_CommunityId_Code",
            schema: "communities",
            table: "StructureNodes",
            columns: new[] { "CommunityId", "Code" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_StructureNodes_ParentId",
            schema: "communities",
            table: "StructureNodes",
            column: "ParentId");

        migrationBuilder.CreateTable(
            name: "CommonAreas",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                IsReservable = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CommonAreas", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_CommonAreas_CommunityId_Code",
            schema: "communities",
            table: "CommonAreas",
            columns: new[] { "CommunityId", "Code" },
            unique: true);

        migrationBuilder.CreateTable(
            name: "BankAccounts",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Bank = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                Number = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Use = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                AccountingAccount = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_BankAccounts", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ChargeSettings",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IssueDay = table.Column<int>(type: "int", nullable: false),
                DueDay = table.Column<int>(type: "int", nullable: false),
                InterestRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                ApplicationOrder = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                FiscalYear = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ChargeSettings", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ChargeSettings_CommunityId",
            schema: "communities",
            table: "ChargeSettings",
            column: "CommunityId",
            unique: true);

        migrationBuilder.CreateTable(
            name: "Authorities",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Role = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                PersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                SupportDocumentReference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Authorities", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Authorities_CommunityId_Role_StartsOn",
            schema: "communities",
            table: "Authorities",
            columns: new[] { "CommunityId", "Role", "StartsOn" });

        migrationBuilder.CreateTable(
            name: "Documents",
            schema: "communities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DocumentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                ExternalReference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                RegisteredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Documents", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Documents_CommunityId",
            schema: "communities",
            table: "Documents",
            column: "CommunityId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Authorities", schema: "communities");
        migrationBuilder.DropTable(name: "BankAccounts", schema: "communities");
        migrationBuilder.DropTable(name: "ChargeSettings", schema: "communities");
        migrationBuilder.DropTable(name: "CommonAreas", schema: "communities");
        migrationBuilder.DropTable(name: "Documents", schema: "communities");
        migrationBuilder.DropTable(name: "StructureNodes", schema: "communities");
        migrationBuilder.DropTable(name: "TaxSettings", schema: "communities");
        migrationBuilder.DropTable(name: "Communities", schema: "communities");
    }
}
