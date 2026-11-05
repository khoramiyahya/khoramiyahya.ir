using PersonalBrand.Infrastructure.Auditing;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.UnitTests.Persistence;

public sealed class ApplicationDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_UsesSqlServerProvider()
    {
        using var dbContext = new ApplicationDbContextFactory().CreateDbContext([]);

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", dbContext.Database.ProviderName);
    }

    [Fact]
    public void AuditLog_Model_Should_Enforce_Bounded_Indexed_Storage()
    {
        using var dbContext = new ApplicationDbContextFactory().CreateDbContext([]);

        var entityType = dbContext.Model.FindEntityType(typeof(AuditLog));

        Assert.NotNull(entityType);
        Assert.Equal(100, entityType.FindProperty(nameof(AuditLog.Action))?.GetMaxLength());
        Assert.Equal(128, entityType.FindProperty(nameof(AuditLog.EntityName))?.GetMaxLength());
        Assert.Equal(450, entityType.FindProperty(nameof(AuditLog.EntityId))?.GetMaxLength());
        Assert.Equal(450, entityType.FindProperty(nameof(AuditLog.UserId))?.GetMaxLength());
        Assert.Equal(4000, entityType.FindProperty(nameof(AuditLog.MetadataJson))?.GetMaxLength());
        Assert.Empty(entityType.GetForeignKeys());
        Assert.Equal(3, entityType.GetIndexes().Count());
    }
}
