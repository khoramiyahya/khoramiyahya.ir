namespace PersonalBrand.Application.Contact.Contracts;

/// <summary>
/// Public contact form submission payload.
/// </summary>
public sealed class SubmitContactRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
}
