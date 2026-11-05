using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using PersonalBrand.Infrastructure.Auditing;

namespace PersonalBrand.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(auditLog => auditLog.EntityName)
            .HasMaxLength(128);

        builder.Property(auditLog => auditLog.EntityId)
            .HasMaxLength(450);

        builder.Property(auditLog => auditLog.UserId)
            .HasMaxLength(450);

        builder.Property(auditLog => auditLog.MetadataJson)
            .HasMaxLength(4000);

        builder.HasIndex(auditLog => auditLog.OccurredOnUtc);
        builder.HasIndex(auditLog => new { auditLog.UserId, auditLog.OccurredOnUtc });
        builder.HasIndex(auditLog => new
        {
            auditLog.EntityName,
            auditLog.EntityId,
            auditLog.OccurredOnUtc
        });
    }
}
