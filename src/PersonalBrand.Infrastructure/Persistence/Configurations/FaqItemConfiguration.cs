using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Faq;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        builder.ToTable("FaqItems");

        builder.Ignore(item => item.DomainEvents);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(item => item.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(item => item.Answer)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(item => item.Category)
            .HasMaxLength(100);

        builder.Property(item => item.DisplayOrder)
            .IsRequired();

        builder.Property(item => item.IsFeatured)
            .IsRequired();

        builder.Property(item => item.IsPublished)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.UserId, item.DisplayOrder });

        builder.HasIndex(item => item.IsPublished);

        builder.HasIndex(item => item.IsFeatured);

        builder.HasIndex(item => item.Category);
    }
}
