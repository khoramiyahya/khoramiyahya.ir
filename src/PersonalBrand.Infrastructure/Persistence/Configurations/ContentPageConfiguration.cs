using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Pages;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.ToTable("ContentPages");

        builder.Ignore(page => page.DomainEvents);

        builder.HasKey(page => page.Id);

        builder.Property(page => page.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(page => page.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(page => page.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(page => page.Summary)
            .HasMaxLength(500);

        builder.Property(page => page.Body)
            .HasMaxLength(50000);

        builder.Property(page => page.MetaTitle)
            .HasMaxLength(200);

        builder.Property(page => page.MetaDescription)
            .HasMaxLength(500);

        builder.Property(page => page.DisplayOrder)
            .IsRequired();

        builder.Property(page => page.IsPublished)
            .IsRequired();

        builder.Property(page => page.CreatedAtUtc)
            .IsRequired();

        builder.Property(page => page.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(page => page.Slug)
            .IsUnique();

        builder.HasIndex(page => new { page.UserId, page.DisplayOrder });

        builder.HasIndex(page => page.IsPublished);
    }
}
