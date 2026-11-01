using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Testimonials;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");

        builder.Ignore(testimonial => testimonial.DomainEvents);

        builder.HasKey(testimonial => testimonial.Id);

        builder.Property(testimonial => testimonial.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(testimonial => testimonial.AuthorName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(testimonial => testimonial.AuthorRole)
            .HasMaxLength(150);

        builder.Property(testimonial => testimonial.Company)
            .HasMaxLength(150);

        builder.Property(testimonial => testimonial.Quote)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(testimonial => testimonial.AuthorImageUrl)
            .HasMaxLength(500);

        builder.Property(testimonial => testimonial.DisplayOrder)
            .IsRequired();

        builder.Property(testimonial => testimonial.IsFeatured)
            .IsRequired();

        builder.Property(testimonial => testimonial.IsPublished)
            .IsRequired();

        builder.Property(testimonial => testimonial.CreatedAtUtc)
            .IsRequired();

        builder.Property(testimonial => testimonial.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(testimonial => new { testimonial.UserId, testimonial.DisplayOrder });

        builder.HasIndex(testimonial => testimonial.IsPublished);

        builder.HasIndex(testimonial => testimonial.IsFeatured);
    }
}
