using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Contact;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");

        builder.Ignore(message => message.DomainEvents);

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(message => message.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(message => message.Phone)
            .HasMaxLength(50);

        builder.Property(message => message.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(message => message.Body)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(message => message.ClientIp)
            .HasMaxLength(100);

        builder.Property(message => message.UserAgent)
            .HasMaxLength(500);

        builder.Property(message => message.IsRead)
            .IsRequired();

        builder.Property(message => message.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(message => message.CreatedAtUtc);

        builder.HasIndex(message => message.IsRead);

        builder.HasIndex(message => message.Email);
    }
}
