using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Settings;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");

        builder.Ignore(settings => settings.DomainEvents);

        builder.HasKey(settings => settings.Id);

        builder.Property(settings => settings.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(settings => settings.SiteName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(settings => settings.Tagline)
            .HasMaxLength(300);

        builder.Property(settings => settings.MetaTitle)
            .HasMaxLength(200);

        builder.Property(settings => settings.MetaDescription)
            .HasMaxLength(500);

        builder.Property(settings => settings.MetaKeywords)
            .HasMaxLength(500);

        builder.Property(settings => settings.CanonicalBaseUrl)
            .HasMaxLength(500);

        builder.Property(settings => settings.OgImageUrl)
            .HasMaxLength(500);

        builder.Property(settings => settings.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(settings => settings.PrimaryEmail)
            .HasMaxLength(256);

        builder.Property(settings => settings.PrimaryPhone)
            .HasMaxLength(50);

        builder.Property(settings => settings.Address)
            .HasMaxLength(500);

        builder.Property(settings => settings.GoogleAnalyticsId)
            .HasMaxLength(50);

        builder.Property(settings => settings.IsMaintenanceMode)
            .IsRequired();

        builder.Property(settings => settings.CreatedAtUtc)
            .IsRequired();

        builder.Property(settings => settings.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(settings => settings.Key)
            .IsUnique();
    }
}
