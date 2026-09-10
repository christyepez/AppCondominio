using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Properties.Infrastructure.Migrations;

[DbContext(typeof(PropertiesDbContext))]
[Migration("20260908133000_InitialProperties")]
public partial class InitialProperties : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "properties");
        migrationBuilder.CreateTable(name: "UnitTypes", schema: "properties", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
            IsActive = table.Column<bool>(type: "bit", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_UnitTypes", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_UnitTypes_CommunityId_Code", schema: "properties", table: "UnitTypes", columns: new[] { "CommunityId", "Code" }, unique: true);

        migrationBuilder.CreateTable(name: "Units", schema: "properties", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            Status = table.Column<int>(type: "int", nullable: false),
            MainAreaM2 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Units", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Units_CommunityId_Code", schema: "properties", table: "Units", columns: new[] { "CommunityId", "Code" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_Units_TypeId", schema: "properties", table: "Units", column: "TypeId");

        migrationBuilder.CreateTable(name: "Relations", schema: "properties", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            MainUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RelatedUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RelationType = table.Column<int>(type: "int", nullable: false),
            StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
            EndsOn = table.Column<DateOnly>(type: "date", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Relations", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Relations_CommunityId_MainUnitId_RelatedUnitId_StartsOn", schema: "properties", table: "Relations", columns: new[] { "CommunityId", "MainUnitId", "RelatedUnitId", "StartsOn" });

        migrationBuilder.CreateTable(name: "Areas", schema: "properties", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Type = table.Column<int>(type: "int", nullable: false),
            AreaM2 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
            IsComputable = table.Column<bool>(type: "bit", nullable: false),
            Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Areas", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Areas_CommunityId_UnitId", schema: "properties", table: "Areas", columns: new[] { "CommunityId", "UnitId" });

        migrationBuilder.CreateTable(name: "AliquotVersions", schema: "properties", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Method = table.Column<int>(type: "int", nullable: false),
            Value = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
            ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
            ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
            Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
            SupportDocumentReference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
            ApprovedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            ApprovedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_AliquotVersions", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_AliquotVersions_CommunityId_UnitId_ValidFrom", schema: "properties", table: "AliquotVersions", columns: new[] { "CommunityId", "UnitId", "ValidFrom" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AliquotVersions", schema: "properties");
        migrationBuilder.DropTable(name: "Areas", schema: "properties");
        migrationBuilder.DropTable(name: "Relations", schema: "properties");
        migrationBuilder.DropTable(name: "Units", schema: "properties");
        migrationBuilder.DropTable(name: "UnitTypes", schema: "properties");
    }
}
