using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Skills;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        builder.Ignore(skill => skill.DomainEvents);

        builder.HasKey(skill => skill.Id);

        builder.Property(skill => skill.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(skill => skill.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(skill => skill.Category)
            .HasMaxLength(100);

        builder.Property(skill => skill.Description)
            .HasMaxLength(1000);

        builder.Property(skill => skill.ProficiencyPercent)
            .IsRequired();

        builder.Property(skill => skill.IconName)
            .HasMaxLength(100);

        builder.Property(skill => skill.DisplayOrder)
            .IsRequired();

        builder.Property(skill => skill.IsFeatured)
            .IsRequired();

        builder.Property(skill => skill.IsPublished)
            .IsRequired();

        builder.Property(skill => skill.CreatedAtUtc)
            .IsRequired();

        builder.Property(skill => skill.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(skill => new { skill.UserId, skill.DisplayOrder });

        builder.HasIndex(skill => skill.IsPublished);

        builder.HasIndex(skill => skill.Category);

        builder.HasIndex(skill => skill.IsFeatured);
    }
}
