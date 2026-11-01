using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Certifications;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("Certifications");

        builder.Ignore(certification => certification.DomainEvents);

        builder.HasKey(certification => certification.Id);

        builder.Property(certification => certification.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(certification => certification.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(certification => certification.Issuer)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(certification => certification.CredentialId)
            .HasMaxLength(200);

        builder.Property(certification => certification.CredentialUrl)
            .HasMaxLength(500);

        builder.Property(certification => certification.Description)
            .HasMaxLength(2000);

        builder.Property(certification => certification.IssuedOn)
            .IsRequired();

        builder.Property(certification => certification.DoesNotExpire)
            .IsRequired();

        builder.Property(certification => certification.DisplayOrder)
            .IsRequired();

        builder.Property(certification => certification.IsFeatured)
            .IsRequired();

        builder.Property(certification => certification.IsPublished)
            .IsRequired();

        builder.Property(certification => certification.CreatedAtUtc)
            .IsRequired();

        builder.Property(certification => certification.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(certification => new { certification.UserId, certification.DisplayOrder });

        builder.HasIndex(certification => certification.IsPublished);

        builder.HasIndex(certification => certification.IsFeatured);

        builder.HasIndex(certification => certification.IssuedOn);
    }
}
