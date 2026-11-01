using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Redirects;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class SeoRedirectConfiguration : IEntityTypeConfiguration<SeoRedirect>
{
    public void Configure(EntityTypeBuilder<SeoRedirect> builder)
    {
        builder.ToTable("SeoRedirects");

        builder.Ignore(redirect => redirect.DomainEvents);

        builder.HasKey(redirect => redirect.Id);

        builder.Property(redirect => redirect.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(redirect => redirect.FromPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(redirect => redirect.ToUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(redirect => redirect.StatusCode)
            .IsRequired();

        builder.Property(redirect => redirect.Notes)
            .HasMaxLength(500);

        builder.Property(redirect => redirect.IsPublished)
            .IsRequired();

        builder.Property(redirect => redirect.CreatedAtUtc)
            .IsRequired();

        builder.Property(redirect => redirect.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(redirect => redirect.FromPath)
            .IsUnique();

        builder.HasIndex(redirect => redirect.IsPublished);

        builder.HasIndex(redirect => redirect.UserId);
    }
}
