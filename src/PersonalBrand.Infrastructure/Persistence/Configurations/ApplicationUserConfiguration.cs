using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Infrastructure.Identity;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.DisplayName)
            .HasMaxLength(200);

        builder.Property(user => user.CreatedAtUtc)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}
