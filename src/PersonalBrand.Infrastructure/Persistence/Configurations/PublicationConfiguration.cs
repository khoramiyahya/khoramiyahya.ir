using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Publications;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.ToTable("Publications");

        builder.Ignore(publication => publication.DomainEvents);

        builder.HasKey(publication => publication.Id);

        builder.Property(publication => publication.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(publication => publication.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(publication => publication.Publisher)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(publication => publication.PublicationType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(publication => publication.Description)
            .HasMaxLength(4000);

        builder.Property(publication => publication.Url)
            .HasMaxLength(500);

        builder.Property(publication => publication.PublishedOn)
            .IsRequired();

        builder.Property(publication => publication.DisplayOrder)
            .IsRequired();

        builder.Property(publication => publication.IsFeatured)
            .IsRequired();

        builder.Property(publication => publication.IsPublished)
            .IsRequired();

        builder.Property(publication => publication.CreatedAtUtc)
            .IsRequired();

        builder.Property(publication => publication.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(publication => new { publication.UserId, publication.DisplayOrder });

        builder.HasIndex(publication => publication.IsPublished);

        builder.HasIndex(publication => publication.IsFeatured);

        builder.HasIndex(publication => publication.PublicationType);

        builder.HasIndex(publication => publication.PublishedOn);
    }
}
