using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Education;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class EducationEntryConfiguration : IEntityTypeConfiguration<EducationEntry>
{
    public void Configure(EntityTypeBuilder<EducationEntry> builder)
    {
        builder.ToTable("EducationEntries");

        builder.Ignore(entry => entry.DomainEvents);

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(entry => entry.Institution)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.Degree)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.FieldOfStudy)
            .HasMaxLength(200);

        builder.Property(entry => entry.Location)
            .HasMaxLength(200);

        builder.Property(entry => entry.Description)
            .HasMaxLength(4000);

        builder.Property(entry => entry.InstitutionUrl)
            .HasMaxLength(500);

        builder.Property(entry => entry.StartedOn)
            .IsRequired();

        builder.Property(entry => entry.IsCurrent)
            .IsRequired();

        builder.Property(entry => entry.DisplayOrder)
            .IsRequired();

        builder.Property(entry => entry.IsFeatured)
            .IsRequired();

        builder.Property(entry => entry.IsPublished)
            .IsRequired();

        builder.Property(entry => entry.CreatedAtUtc)
            .IsRequired();

        builder.Property(entry => entry.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(entry => new { entry.UserId, entry.DisplayOrder });

        builder.HasIndex(entry => entry.IsPublished);

        builder.HasIndex(entry => entry.IsFeatured);

        builder.HasIndex(entry => entry.StartedOn);
    }
}
