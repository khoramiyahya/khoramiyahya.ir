using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Services;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class ServiceOfferConfiguration : IEntityTypeConfiguration<ServiceOffer>
{
    public void Configure(EntityTypeBuilder<ServiceOffer> builder)
    {
        builder.ToTable("ServiceOffers");

        builder.Ignore(service => service.DomainEvents);

        builder.HasKey(service => service.Id);

        builder.Property(service => service.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(service => service.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(service => service.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(service => service.Summary)
            .HasMaxLength(500);

        builder.Property(service => service.Description)
            .HasMaxLength(8000);

        builder.Property(service => service.PriceLabel)
            .HasMaxLength(100);

        builder.Property(service => service.IconName)
            .HasMaxLength(100);

        builder.Property(service => service.DisplayOrder)
            .IsRequired();

        builder.Property(service => service.IsFeatured)
            .IsRequired();

        builder.Property(service => service.IsPublished)
            .IsRequired();

        builder.Property(service => service.CreatedAtUtc)
            .IsRequired();

        builder.Property(service => service.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(service => service.Slug)
            .IsUnique();

        builder.HasIndex(service => new { service.UserId, service.DisplayOrder });

        builder.HasIndex(service => service.IsPublished);

        builder.HasIndex(service => service.IsFeatured);
    }
}
