using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppCondominio.Modules.Organizations.Infrastructure.Migrations;

[DbContext(typeof(OrganizationsDbContext))]
partial class OrganizationsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity<Organization>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<string>("Address").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("CommercialName").IsRequired().HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("Email").HasMaxLength(320).HasColumnType("nvarchar(320)");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<string>("LegalName").IsRequired().HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("LegalRepresentative").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("Phone").HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.Property<DateTimeOffset?>("SuspendedAtUtc").HasColumnType("datetimeoffset");
            b.Property<string>("SuspensionReason").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("TaxId").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.HasKey("Id"); b.HasIndex("Name"); b.HasIndex("TaxId"); b.ToTable("Organizations", "organizations");
        });

        modelBuilder.Entity<CommercialPlan>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<int>("BillingPeriod").HasColumnType("int");
            b.Property<string>("Code").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<string>("Currency").IsRequired().HasMaxLength(3).HasColumnType("nvarchar(3)");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<long>("MaxStorageMb").HasColumnType("bigint");
            b.Property<int>("MaxUnits").HasColumnType("int");
            b.Property<int>("MaxUsers").HasColumnType("int");
            b.Property<string>("ModulesCsv").IsRequired().HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            b.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.Property<decimal>("Price").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            b.HasKey("Id"); b.HasIndex("Code").IsUnique(); b.ToTable("CommercialPlans", "organizations");
        });

        modelBuilder.Entity<Subscription>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<string>("CommercialNotes").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("ModulesCsv").IsRequired().HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            b.Property<Guid>("OrganizationId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("PlanId").HasColumnType("uniqueidentifier");
            b.Property<DateTimeOffset>("RenewsAtUtc").HasColumnType("datetimeoffset");
            b.Property<DateTimeOffset>("StartsAtUtc").HasColumnType("datetimeoffset");
            b.Property<int>("Status").HasColumnType("int");
            b.HasKey("Id"); b.HasIndex("OrganizationId").IsUnique(); b.HasIndex("PlanId"); b.ToTable("Subscriptions", "organizations");
        });

        modelBuilder.Entity<BrandSettings>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<string>("ContactEmail").HasMaxLength(320).HasColumnType("nvarchar(320)");
            b.Property<string>("ContactPhone").HasMaxLength(60).HasColumnType("nvarchar(60)");
            b.Property<string>("FaviconUrl").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("LogoUrl").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid>("OrganizationId").HasColumnType("uniqueidentifier");
            b.Property<string>("PrimaryColor").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.Property<string>("ProductName").IsRequired().HasMaxLength(160).HasColumnType("nvarchar(160)");
            b.Property<string>("SecondaryColor").IsRequired().HasMaxLength(16).HasColumnType("nvarchar(16)");
            b.HasKey("Id"); b.HasIndex("OrganizationId").IsUnique(); b.ToTable("BrandSettings", "organizations");
        });

        modelBuilder.Entity<TenantDatabaseProfile>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedNever().HasColumnType("uniqueidentifier");
            b.Property<string>("ConnectionSecretReference").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("MigrationStatus").IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            b.Property<Guid>("OrganizationId").HasColumnType("uniqueidentifier");
            b.Property<int>("Strategy").HasColumnType("int");
            b.HasKey("Id"); b.HasIndex("OrganizationId").IsUnique(); b.ToTable("TenantDatabaseProfiles", "organizations");
        });
#pragma warning restore 612, 618
    }
}
