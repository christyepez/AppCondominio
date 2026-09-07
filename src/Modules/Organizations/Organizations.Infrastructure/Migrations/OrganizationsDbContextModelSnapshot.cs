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
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uniqueidentifier");

            b.Property<bool>("IsActive")
                .HasColumnType("bit");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<string>("TaxId")
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)");

            b.HasKey("Id");
            b.HasIndex("Name");
            b.HasIndex("TaxId");
            b.ToTable("Organizations", "organizations");
        });
#pragma warning restore 612, 618
    }
}
