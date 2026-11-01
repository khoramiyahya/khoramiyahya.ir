using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Awards;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class AwardConfiguration : IEntityTypeConfiguration<Award>
{
    public void Configure(EntityTypeBuilder<Award> builder)
    {
        builder.ToTable("Awards");

        builder.Ignore(award => award.DomainEvents);

        builder.HasKey(award => award.Id);

        builder.Property(award => award.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(award => award.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(award => award.Issuer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(award => award.Description)
            .HasMaxLength(2000);

        builder.Property(award => award.Url)
            .HasMaxLength(500);

        builder.Property(award => award.AwardedOn)
            .IsRequired();

        builder.Property(award => award.DisplayOrder)
            .IsRequired();

        builder.Property(award => award.IsFeatured)
            .IsRequired();

        builder.Property(award => award.IsPublished)
            .IsRequired();

        builder.Property(award => award.CreatedAtUtc)
            .IsRequired();

        builder.Property(award => award.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(award => new { award.UserId, award.DisplayOrder });

        builder.HasIndex(award => award.IsPublished);

        builder.HasIndex(award => award.IsFeatured);

        builder.HasIndex(award => award.AwardedOn);
    }
}
