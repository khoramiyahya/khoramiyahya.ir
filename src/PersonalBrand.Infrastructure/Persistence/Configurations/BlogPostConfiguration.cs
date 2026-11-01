using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Blog;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");

        builder.Ignore(post => post.DomainEvents);

        builder.HasKey(post => post.Id);

        builder.Property(post => post.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(post => post.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(post => post.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(post => post.Summary)
            .HasMaxLength(500);

        builder.Property(post => post.Body)
            .HasMaxLength(50000);

        builder.Property(post => post.CoverImagePath)
            .HasMaxLength(500);

        builder.Property(post => post.Tags)
            .HasMaxLength(500);

        builder.Property(post => post.IsFeatured)
            .IsRequired();

        builder.Property(post => post.IsPublished)
            .IsRequired();

        builder.Property(post => post.CreatedAtUtc)
            .IsRequired();

        builder.Property(post => post.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(post => post.Slug)
            .IsUnique();

        builder.HasIndex(post => new { post.UserId, post.UpdatedAtUtc });

        builder.HasIndex(post => post.IsPublished);

        builder.HasIndex(post => post.IsFeatured);

        builder.HasIndex(post => post.PublishedAtUtc);
    }
}
