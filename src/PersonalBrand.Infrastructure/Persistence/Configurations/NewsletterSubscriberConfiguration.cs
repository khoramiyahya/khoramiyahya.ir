using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Newsletter;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("NewsletterSubscribers");

        builder.Ignore(subscriber => subscriber.DomainEvents);

        builder.HasKey(subscriber => subscriber.Id);

        builder.Property(subscriber => subscriber.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(subscriber => subscriber.Name)
            .HasMaxLength(150);

        builder.Property(subscriber => subscriber.IsActive)
            .IsRequired();

        builder.Property(subscriber => subscriber.ClientIp)
            .HasMaxLength(100);

        builder.Property(subscriber => subscriber.UserAgent)
            .HasMaxLength(500);

        builder.Property(subscriber => subscriber.SubscribedAtUtc)
            .IsRequired();

        builder.Property(subscriber => subscriber.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(subscriber => subscriber.Email)
            .IsUnique();

        builder.HasIndex(subscriber => subscriber.IsActive);

        builder.HasIndex(subscriber => subscriber.SubscribedAtUtc);
    }
}
