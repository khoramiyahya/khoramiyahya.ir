namespace PersonalBrand.Application.Certifications.Contracts;

/// <summary>
/// Public representation of a certification.
/// </summary>
public sealed class CertificationDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Issuer { get; init; }

    public string? CredentialId { get; init; }

    public string? CredentialUrl { get; init; }

    public string? Description { get; init; }

    public required DateOnly IssuedOn { get; init; }

    public DateOnly? ExpiresOn { get; init; }

    public required bool DoesNotExpire { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}
