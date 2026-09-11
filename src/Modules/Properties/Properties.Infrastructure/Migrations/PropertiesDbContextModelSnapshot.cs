using AppCondominio.Modules.Properties.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Properties.Infrastructure.Migrations;

[DbContext(typeof(PropertiesDbContext))]
partial class PropertiesDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "10.0.11").HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity<PropertyUnitType>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier");
            b.Property<string>("Code").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.HasKey("Id"); b.HasIndex("CommunityId", "Code").IsUnique(); b.ToTable("UnitTypes", "properties");
        });
        modelBuilder.Entity<PropertyUnit>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("TypeId").HasColumnType("uniqueidentifier");
            b.Property<string>("Code").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<string>("Location").IsRequired().HasMaxLength(300).HasColumnType("nvarchar(300)");
            b.Property<PropertyUnitStatus>("Status").HasColumnType("int");
            b.Property<decimal>("MainAreaM2").HasPrecision(18,4).HasColumnType("decimal(18,4)");
            b.Property<string>("Notes").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.HasKey("Id"); b.HasIndex("CommunityId", "Code").IsUnique(); b.HasIndex("TypeId"); b.ToTable("Units", "properties");
        });
        modelBuilder.Entity<PropertyRelation>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("MainUnitId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("RelatedUnitId").HasColumnType("uniqueidentifier");
            b.Property<PropertyRelationType>("RelationType").HasColumnType("int");
            b.Property<DateOnly>("StartsOn").HasColumnType("date");
            b.Property<DateOnly?>("EndsOn").HasColumnType("date");
            b.HasKey("Id"); b.HasIndex("CommunityId", "MainUnitId", "RelatedUnitId", "StartsOn"); b.ToTable("Relations", "properties");
        });
        modelBuilder.Entity<PropertyArea>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("UnitId").HasColumnType("uniqueidentifier");
            b.Property<PropertyAreaType>("Type").HasColumnType("int");
            b.Property<decimal>("AreaM2").HasPrecision(18,4).HasColumnType("decimal(18,4)");
            b.Property<bool>("IsComputable").HasColumnType("bit");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.HasKey("Id"); b.HasIndex("CommunityId", "UnitId"); b.ToTable("Areas", "properties");
        });
        modelBuilder.Entity<AliquotVersion>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("UnitId").HasColumnType("uniqueidentifier");
            b.Property<AliquotCalculationMethod>("Method").HasColumnType("int");
            b.Property<decimal>("Value").HasPrecision(18,6).HasColumnType("decimal(18,6)");
            b.Property<DateOnly>("ValidFrom").HasColumnType("date");
            b.Property<DateOnly?>("ValidTo").HasColumnType("date");
            b.Property<string>("Reason").IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("SupportDocumentReference").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("ApprovedBy").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<DateTimeOffset?>("ApprovedAtUtc").HasColumnType("datetimeoffset");
            b.HasKey("Id"); b.HasIndex("CommunityId", "UnitId", "ValidFrom"); b.ToTable("AliquotVersions", "properties");
        });
#pragma warning restore 612, 618
    }
}
