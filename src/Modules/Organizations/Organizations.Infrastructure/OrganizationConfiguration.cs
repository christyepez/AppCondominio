using AppCondominio.Modules.Organizations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppCondominio.Modules.Organizations.Infrastructure;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", "organizations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TaxId)
            .HasMaxLength(20);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.TaxId);
    }
}
