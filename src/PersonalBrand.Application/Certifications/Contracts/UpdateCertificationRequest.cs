namespace PersonalBrand.Application.Certifications.Contracts;

/// <summary>
/// Request payload for updating a certification.
/// </summary>
public sealed class UpdateCertificationRequest
{
    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string? CredentialId { get; set; }

    public string? CredentialUrl { get; set; }

    public string? Description { get; set; }

    public DateOnly IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public bool DoesNotExpire { get; set; } = true;

    public int DisplayOrder { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}
