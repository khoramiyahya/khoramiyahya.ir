namespace PersonalBrand.Application.Redirects.Contracts;

/// <summary>
/// Request payload for creating an SEO redirect.
/// </summary>
public sealed class CreateSeoRedirectRequest
{
    public string FromPath { get; set; } = string.Empty;

    public string ToUrl { get; set; } = string.Empty;

    public int StatusCode { get; set; } = 301;

    public string? Notes { get; set; }

    public bool IsPublished { get; set; }
}
