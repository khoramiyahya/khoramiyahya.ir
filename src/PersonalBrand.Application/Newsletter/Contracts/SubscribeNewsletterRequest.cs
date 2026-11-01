namespace PersonalBrand.Application.Newsletter.Contracts;

/// <summary>
/// Public newsletter subscription payload.
/// </summary>
public sealed class SubscribeNewsletterRequest
{
    public string Email { get; set; } = string.Empty;

    public string? Name { get; set; }
}
