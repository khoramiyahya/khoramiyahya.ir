using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Robots;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class RobotsRuleConfiguration : IEntityTypeConfiguration<RobotsRule>
{
    public void Configure(EntityTypeBuilder<RobotsRule> builder)
    {
        builder.ToTable("RobotsRules");

        builder.Ignore(rule => rule.DomainEvents);

        builder.HasKey(rule => rule.Id);

        builder.Property(rule => rule.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rule => rule.UserAgent)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rule => rule.Directive)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(rule => rule.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rule => rule.DisplayOrder)
            .IsRequired();

        builder.Property(rule => rule.IsPublished)
            .IsRequired();

        builder.Property(rule => rule.CreatedAtUtc)
            .IsRequired();

        builder.Property(rule => rule.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(rule => new { rule.UserId, rule.DisplayOrder });

        builder.HasIndex(rule => rule.IsPublished);

        builder.HasIndex(rule => rule.UserAgent);
    }
}
