using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Profiles;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class BrandProfileConfiguration : IEntityTypeConfiguration<BrandProfile>
{
    public void Configure(EntityTypeBuilder<BrandProfile> builder)
    {
        builder.ToTable("BrandProfiles");

        builder.Ignore(profile => profile.DomainEvents);

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(profile => profile.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(profile => profile.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(profile => profile.Headline)
            .HasMaxLength(300);

        builder.Property(profile => profile.Bio)
            .HasMaxLength(4000);

        builder.Property(profile => profile.AvatarPath)
            .HasMaxLength(500);

        builder.Property(profile => profile.Location)
            .HasMaxLength(200);

        builder.Property(profile => profile.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(profile => profile.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(profile => profile.GitHubUrl)
            .HasMaxLength(500);

        builder.Property(profile => profile.TwitterUrl)
            .HasMaxLength(500);

        builder.Property(profile => profile.InstagramUrl)
            .HasMaxLength(500);

        builder.Property(profile => profile.IsPublished)
            .IsRequired();

        builder.Property(profile => profile.CreatedAtUtc)
            .IsRequired();

        builder.Property(profile => profile.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();

        builder.HasIndex(profile => profile.Slug)
            .IsUnique();

        builder.HasIndex(profile => profile.IsPublished);
    }
}
