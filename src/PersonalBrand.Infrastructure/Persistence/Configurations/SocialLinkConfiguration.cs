using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.SocialLinks;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class SocialLinkConfiguration : IEntityTypeConfiguration<SocialLink>
{
    public void Configure(EntityTypeBuilder<SocialLink> builder)
    {
        builder.ToTable("SocialLinks");

        builder.Ignore(link => link.DomainEvents);

        builder.HasKey(link => link.Id);

        builder.Property(link => link.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(link => link.Platform)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(link => link.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(link => link.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(link => link.Icon)
            .HasMaxLength(100);

        builder.Property(link => link.DisplayOrder)
            .IsRequired();

        builder.Property(link => link.IsPublished)
            .IsRequired();

        builder.Property(link => link.CreatedAtUtc)
            .IsRequired();

        builder.Property(link => link.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(link => new { link.UserId, link.DisplayOrder });

        builder.HasIndex(link => link.IsPublished);

        builder.HasIndex(link => link.Platform);
    }
}
