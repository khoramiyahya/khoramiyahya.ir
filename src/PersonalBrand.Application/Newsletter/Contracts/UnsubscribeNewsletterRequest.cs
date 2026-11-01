namespace PersonalBrand.Application.Newsletter.Contracts;

/// <summary>
/// Public newsletter unsubscribe payload.
/// </summary>
public sealed class UnsubscribeNewsletterRequest
{
    public string Email { get; set; } = string.Empty;
}
