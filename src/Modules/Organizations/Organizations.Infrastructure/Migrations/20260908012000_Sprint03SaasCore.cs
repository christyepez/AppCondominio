using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Organizations.Infrastructure.Migrations;

[DbContext(typeof(OrganizationsDbContext))]
[Migration("20260908012000_Sprint03SaasCore")]
public partial class Sprint03SaasCore : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Address", schema: "organizations", table: "Organizations", type: "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>(name: "CommercialName", schema: "organizations", table: "Organizations", type: "nvarchar(250)", maxLength: 250, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>(name: "Email", schema: "organizations", table: "Organizations", type: "nvarchar(320)", maxLength: 320, nullable: true);
        migrationBuilder.AddColumn<string>(name: "LegalName", schema: "organizations", table: "Organizations", type: "nvarchar(250)", maxLength: 250, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>(name: "LegalRepresentative", schema: "organizations", table: "Organizations", type: "nvarchar(250)", maxLength: 250, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Phone", schema: "organizations", table: "Organizations", type: "nvarchar(60)", maxLength: 60, nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "SuspendedAtUtc", schema: "organizations", table: "Organizations", type: "datetimeoffset", nullable: true);
        migrationBuilder.AddColumn<string>(name: "SuspensionReason", schema: "organizations", table: "Organizations", type: "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.Sql("UPDATE organizations.Organizations SET LegalName = Name, CommercialName = Name WHERE LegalName = '' OR CommercialName = '';");

        migrationBuilder.CreateTable(
            name: "CommercialPlans", schema: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), MaxUnits = table.Column<int>(type: "int", nullable: false),
                MaxUsers = table.Column<int>(type: "int", nullable: false), MaxStorageMb = table.Column<long>(type: "bigint", nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false), Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                BillingPeriod = table.Column<int>(type: "int", nullable: false), ModulesCsv = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_CommercialPlans", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_CommercialPlans_Code", schema: "organizations", table: "CommercialPlans", column: "Code", unique: true);

        migrationBuilder.CreateTable(
            name: "Subscriptions", schema: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false), StartsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RenewsAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false), Status = table.Column<int>(type: "int", nullable: false),
                ModulesCsv = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false), CommercialNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
            }, constraints: table => table.PrimaryKey("PK_Subscriptions", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Subscriptions_OrganizationId", schema: "organizations", table: "Subscriptions", column: "OrganizationId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Subscriptions_PlanId", schema: "organizations", table: "Subscriptions", column: "PlanId");

        migrationBuilder.CreateTable(
            name: "BrandSettings", schema: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false), LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                FaviconUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true), PrimaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                SecondaryColor = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false), ContactEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                ContactPhone = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
            }, constraints: table => table.PrimaryKey("PK_BrandSettings", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_BrandSettings_OrganizationId", schema: "organizations", table: "BrandSettings", column: "OrganizationId", unique: true);

        migrationBuilder.CreateTable(
            name: "TenantDatabaseProfiles", schema: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Strategy = table.Column<int>(type: "int", nullable: false), ConnectionSecretReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                MigrationStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_TenantDatabaseProfiles", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_TenantDatabaseProfiles_OrganizationId", schema: "organizations", table: "TenantDatabaseProfiles", column: "OrganizationId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BrandSettings", schema: "organizations");
        migrationBuilder.DropTable(name: "CommercialPlans", schema: "organizations");
        migrationBuilder.DropTable(name: "Subscriptions", schema: "organizations");
        migrationBuilder.DropTable(name: "TenantDatabaseProfiles", schema: "organizations");
        foreach (var column in new[] { "Address", "CommercialName", "Email", "LegalName", "LegalRepresentative", "Phone", "SuspendedAtUtc", "SuspensionReason" })
            migrationBuilder.DropColumn(name: column, schema: "organizations", table: "Organizations");
    }
}
