using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Domain.Talks;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class TalkConfiguration : IEntityTypeConfiguration<Talk>
{
    public void Configure(EntityTypeBuilder<Talk> builder)
    {
        builder.ToTable("Talks");

        builder.Ignore(talk => talk.DomainEvents);

        builder.HasKey(talk => talk.Id);

        builder.Property(talk => talk.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(talk => talk.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(talk => talk.EventName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(talk => talk.Location)
            .HasMaxLength(200);

        builder.Property(talk => talk.Description)
            .HasMaxLength(4000);

        builder.Property(talk => talk.Url)
            .HasMaxLength(500);

        builder.Property(talk => talk.SpokenOn)
            .IsRequired();

        builder.Property(talk => talk.DisplayOrder)
            .IsRequired();

        builder.Property(talk => talk.IsFeatured)
            .IsRequired();

        builder.Property(talk => talk.IsPublished)
            .IsRequired();

        builder.Property(talk => talk.CreatedAtUtc)
            .IsRequired();

        builder.Property(talk => talk.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(talk => new { talk.UserId, talk.DisplayOrder });

        builder.HasIndex(talk => talk.IsPublished);

        builder.HasIndex(talk => talk.IsFeatured);

        builder.HasIndex(talk => talk.SpokenOn);
    }
}
