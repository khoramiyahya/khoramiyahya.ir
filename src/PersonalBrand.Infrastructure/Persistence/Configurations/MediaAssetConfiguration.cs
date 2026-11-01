using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Media;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");

        builder.Ignore(asset => asset.DomainEvents);

        builder.HasKey(asset => asset.Id);

        builder.Property(asset => asset.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(asset => asset.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(asset => asset.StoredPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(asset => asset.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(asset => asset.SizeBytes)
            .IsRequired();

        builder.Property(asset => asset.Title)
            .HasMaxLength(200);

        builder.Property(asset => asset.AltText)
            .HasMaxLength(300);

        builder.Property(asset => asset.CreatedAtUtc)
            .IsRequired();

        builder.Property(asset => asset.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(asset => new { asset.UserId, asset.CreatedAtUtc });

        builder.HasIndex(asset => asset.StoredPath);
    }
}
