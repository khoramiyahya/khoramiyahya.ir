using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Languages;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class SpokenLanguageConfiguration : IEntityTypeConfiguration<SpokenLanguage>
{
    public void Configure(EntityTypeBuilder<SpokenLanguage> builder)
    {
        builder.ToTable("SpokenLanguages");

        builder.Ignore(language => language.DomainEvents);

        builder.HasKey(language => language.Id);

        builder.Property(language => language.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(language => language.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(language => language.Proficiency)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(language => language.DisplayOrder)
            .IsRequired();

        builder.Property(language => language.IsFeatured)
            .IsRequired();

        builder.Property(language => language.IsPublished)
            .IsRequired();

        builder.Property(language => language.CreatedAtUtc)
            .IsRequired();

        builder.Property(language => language.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(language => new { language.UserId, language.Name })
            .IsUnique();

        builder.HasIndex(language => new { language.UserId, language.DisplayOrder });

        builder.HasIndex(language => language.IsPublished);

        builder.HasIndex(language => language.Proficiency);
    }
}
