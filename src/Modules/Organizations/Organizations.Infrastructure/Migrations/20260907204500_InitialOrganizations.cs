using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Organizations.Infrastructure.Migrations;

[DbContext(typeof(OrganizationsDbContext))]
[Migration("20260907204500_InitialOrganizations")]
public partial class InitialOrganizations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "organizations");

        migrationBuilder.CreateTable(
            name: "Organizations",
            schema: "organizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_Name",
            schema: "organizations",
            table: "Organizations",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_TaxId",
            schema: "organizations",
            table: "Organizations",
            column: "TaxId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Organizations",
            schema: "organizations");
    }
}
