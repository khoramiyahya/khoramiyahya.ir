using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Partners;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners");

        builder.Ignore(partner => partner.DomainEvents);

        builder.HasKey(partner => partner.Id);

        builder.Property(partner => partner.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(partner => partner.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(partner => partner.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(partner => partner.LogoUrl)
            .HasMaxLength(500);

        builder.Property(partner => partner.Summary)
            .HasMaxLength(500);

        builder.Property(partner => partner.DisplayOrder)
            .IsRequired();

        builder.Property(partner => partner.IsFeatured)
            .IsRequired();

        builder.Property(partner => partner.IsPublished)
            .IsRequired();

        builder.Property(partner => partner.CreatedAtUtc)
            .IsRequired();

        builder.Property(partner => partner.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(partner => new { partner.UserId, partner.DisplayOrder });

        builder.HasIndex(partner => partner.IsPublished);

        builder.HasIndex(partner => partner.IsFeatured);
    }
}
