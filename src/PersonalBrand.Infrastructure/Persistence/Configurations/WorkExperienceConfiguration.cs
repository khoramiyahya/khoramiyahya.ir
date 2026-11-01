using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Experience;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class WorkExperienceConfiguration : IEntityTypeConfiguration<WorkExperience>
{
    public void Configure(EntityTypeBuilder<WorkExperience> builder)
    {
        builder.ToTable("WorkExperiences");

        builder.Ignore(experience => experience.DomainEvents);

        builder.HasKey(experience => experience.Id);

        builder.Property(experience => experience.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(experience => experience.Company)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(experience => experience.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(experience => experience.Location)
            .HasMaxLength(200);

        builder.Property(experience => experience.Description)
            .HasMaxLength(4000);

        builder.Property(experience => experience.CompanyUrl)
            .HasMaxLength(500);

        builder.Property(experience => experience.StartedOn)
            .IsRequired();

        builder.Property(experience => experience.IsCurrent)
            .IsRequired();

        builder.Property(experience => experience.DisplayOrder)
            .IsRequired();

        builder.Property(experience => experience.IsFeatured)
            .IsRequired();

        builder.Property(experience => experience.IsPublished)
            .IsRequired();

        builder.Property(experience => experience.CreatedAtUtc)
            .IsRequired();

        builder.Property(experience => experience.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(experience => new { experience.UserId, experience.DisplayOrder });

        builder.HasIndex(experience => experience.IsPublished);

        builder.HasIndex(experience => experience.IsFeatured);

        builder.HasIndex(experience => experience.StartedOn);
    }
}
