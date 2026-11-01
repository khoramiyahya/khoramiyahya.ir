namespace PersonalBrand.Infrastructure.Identity;

/// <summary>
/// Persisted refresh token. Only the hash of the raw token is stored.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public ApplicationUser? User { get; set; }

    /// <summary>
    /// SHA-256 hash of the opaque refresh token value.
    /// </summary>
    public required string TokenHash { get; set; }

    /// <summary>
    /// Groups rotated tokens so reuse of a revoked token can invalidate the whole chain.
    /// </summary>
    public Guid FamilyId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsExpired(DateTimeOffset utcNow) => ExpiresAtUtc <= utcNow;

    public bool IsActive(DateTimeOffset utcNow) => !IsRevoked && !IsExpired(utcNow);
}
