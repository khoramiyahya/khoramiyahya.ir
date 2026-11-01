using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Projects;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioProjectConfiguration : IEntityTypeConfiguration<PortfolioProject>
{
    public void Configure(EntityTypeBuilder<PortfolioProject> builder)
    {
        builder.ToTable("PortfolioProjects");

        builder.Ignore(project => project.DomainEvents);

        builder.HasKey(project => project.Id);

        builder.Property(project => project.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(project => project.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(project => project.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(project => project.Summary)
            .HasMaxLength(500);

        builder.Property(project => project.Description)
            .HasMaxLength(8000);

        builder.Property(project => project.CoverImagePath)
            .HasMaxLength(500);

        builder.Property(project => project.ProjectUrl)
            .HasMaxLength(500);

        builder.Property(project => project.RepoUrl)
            .HasMaxLength(500);

        builder.Property(project => project.Technologies)
            .HasMaxLength(500);

        builder.Property(project => project.DisplayOrder)
            .IsRequired();

        builder.Property(project => project.IsFeatured)
            .IsRequired();

        builder.Property(project => project.IsPublished)
            .IsRequired();

        builder.Property(project => project.CreatedAtUtc)
            .IsRequired();

        builder.Property(project => project.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(project => project.Slug)
            .IsUnique();

        builder.HasIndex(project => new { project.UserId, project.DisplayOrder });

        builder.HasIndex(project => project.IsPublished);

        builder.HasIndex(project => project.IsFeatured);
    }
}
