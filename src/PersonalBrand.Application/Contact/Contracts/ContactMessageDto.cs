namespace PersonalBrand.Application.Contact.Contracts;

/// <summary>
/// Inbox representation of a contact message.
/// </summary>
public sealed class ContactMessageDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public string? Phone { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public required bool IsRead { get; init; }

    public DateTimeOffset? ReadAtUtc { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }
}
