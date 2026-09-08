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
        migrationBuilder.AddColumn<string>("Address", "Organizations", "organizations", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<string>("CommercialName", "Organizations", "organizations", "nvarchar(250)", maxLength: 250, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>("Email", "Organizations", "organizations", "nvarchar(320)", maxLength: 320, nullable: true);
        migrationBuilder.AddColumn<string>("LegalName", "Organizations", "organizations", "nvarchar(250)", maxLength: 250, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>("LegalRepresentative", "Organizations", "organizations", "nvarchar(250)", maxLength: 250, nullable: true);
        migrationBuilder.AddColumn<string>("Phone", "Organizations", "organizations", "nvarchar(60)", maxLength: 60, nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset>("SuspendedAtUtc", "Organizations", "organizations", nullable: true);
        migrationBuilder.AddColumn<string>("SuspensionReason", "Organizations", "organizations", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.Sql("UPDATE organizations.Organizations SET LegalName = Name, CommercialName = Name WHERE LegalName = '' OR CommercialName = '';");

        migrationBuilder.CreateTable("CommercialPlans", "organizations", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false), Code = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false),
            Name = table.Column<string>("nvarchar(160)", maxLength: 160, nullable: false), MaxUnits = table.Column<int>("int", nullable: false),
            MaxUsers = table.Column<int>("int", nullable: false), MaxStorageMb = table.Column<long>("bigint", nullable: false),
            Price = table.Column<decimal>("decimal(18,2)", nullable: false), Currency = table.Column<string>("nvarchar(3)", maxLength: 3, nullable: false),
            BillingPeriod = table.Column<int>("int", nullable: false), ModulesCsv = table.Column<string>("nvarchar(2000)", maxLength: 2000, nullable: false),
            IsActive = table.Column<bool>("bit", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_CommercialPlans", x => x.Id));
        migrationBuilder.CreateIndex("IX_CommercialPlans_Code", "CommercialPlans", "organizations", "Code", unique: true);

        migrationBuilder.CreateTable("Subscriptions", "organizations", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>("uniqueidentifier", nullable: false),
            PlanId = table.Column<Guid>("uniqueidentifier", nullable: false), StartsAtUtc = table.Column<DateTimeOffset>("datetimeoffset", nullable: false),
            RenewsAtUtc = table.Column<DateTimeOffset>("datetimeoffset", nullable: false), Status = table.Column<int>("int", nullable: false),
            ModulesCsv = table.Column<string>("nvarchar(2000)", maxLength: 2000, nullable: false), CommercialNotes = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Subscriptions", x => x.Id));
        migrationBuilder.CreateIndex("IX_Subscriptions_OrganizationId", "Subscriptions", "organizations", "OrganizationId", unique: true);
        migrationBuilder.CreateIndex("IX_Subscriptions_PlanId", "Subscriptions", "organizations", "PlanId");

        migrationBuilder.CreateTable("BrandSettings", "organizations", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>("uniqueidentifier", nullable: false),
            ProductName = table.Column<string>("nvarchar(160)", maxLength: 160, nullable: false), LogoUrl = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
            FaviconUrl = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true), PrimaryColor = table.Column<string>("nvarchar(16)", maxLength: 16, nullable: false),
            SecondaryColor = table.Column<string>("nvarchar(16)", maxLength: 16, nullable: false), ContactEmail = table.Column<string>("nvarchar(320)", maxLength: 320, nullable: true),
            ContactPhone = table.Column<string>("nvarchar(60)", maxLength: 60, nullable: true)
        }, constraints: table => table.PrimaryKey("PK_BrandSettings", x => x.Id));
        migrationBuilder.CreateIndex("IX_BrandSettings_OrganizationId", "BrandSettings", "organizations", "OrganizationId", unique: true);

        migrationBuilder.CreateTable("TenantDatabaseProfiles", "organizations", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false), OrganizationId = table.Column<Guid>("uniqueidentifier", nullable: false),
            Strategy = table.Column<int>("int", nullable: false), ConnectionSecretReference = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: true),
            MigrationStatus = table.Column<string>("nvarchar(80)", maxLength: 80, nullable: false)
        }, constraints: table => table.PrimaryKey("PK_TenantDatabaseProfiles", x => x.Id));
        migrationBuilder.CreateIndex("IX_TenantDatabaseProfiles_OrganizationId", "TenantDatabaseProfiles", "organizations", "OrganizationId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("BrandSettings", "organizations");
        migrationBuilder.DropTable("CommercialPlans", "organizations");
        migrationBuilder.DropTable("Subscriptions", "organizations");
        migrationBuilder.DropTable("TenantDatabaseProfiles", "organizations");
        migrationBuilder.DropColumn("Address", "Organizations", "organizations");
        migrationBuilder.DropColumn("CommercialName", "Organizations", "organizations");
        migrationBuilder.DropColumn("Email", "Organizations", "organizations");
        migrationBuilder.DropColumn("LegalName", "Organizations", "organizations");
        migrationBuilder.DropColumn("LegalRepresentative", "Organizations", "organizations");
        migrationBuilder.DropColumn("Phone", "Organizations", "organizations");
        migrationBuilder.DropColumn("SuspendedAtUtc", "Organizations", "organizations");
        migrationBuilder.DropColumn("SuspensionReason", "Organizations", "organizations");
    }
}
