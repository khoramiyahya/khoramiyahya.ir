using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Navigation;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class NavigationItemConfiguration : IEntityTypeConfiguration<NavigationItem>
{
    public void Configure(EntityTypeBuilder<NavigationItem> builder)
    {
        builder.ToTable("NavigationItems");

        builder.Ignore(item => item.DomainEvents);

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(item => item.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(item => item.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(item => item.Location)
            .HasMaxLength(50);

        builder.Property(item => item.OpensInNewTab)
            .IsRequired();

        builder.Property(item => item.DisplayOrder)
            .IsRequired();

        builder.Property(item => item.IsPublished)
            .IsRequired();

        builder.Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder.Property(item => item.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(item => new { item.UserId, item.DisplayOrder });

        builder.HasIndex(item => item.IsPublished);

        builder.HasIndex(item => item.Location);
    }
}
