using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Languages;

/// <summary>
/// Spoken language entry owned by a user account.
/// </summary>
public sealed class SpokenLanguage : AggregateRoot<Guid>
{
    private static readonly HashSet<string> AllowedProficiencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "native",
        "fluent",
        "professional",
        "intermediate",
        "basic"
    };

    private SpokenLanguage()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Proficiency { get; private set; } = "professional";

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SpokenLanguage Create(
        string userId,
        string name,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Language owner is required.");
        }

        var language = new SpokenLanguage
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsFeatured = false,
            IsPublished = false
        };

        language.ApplyContent(name, "professional");
        return language;
    }

    public void UpdateDetails(
        string name,
        string proficiency,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyContent(name, proficiency);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Proficiency))
        {
            throw new DomainException("A published language requires a name and proficiency.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(string name, string proficiency)
    {
        Name = NormalizeRequiredText(name, "Name", 100);
        Proficiency = NormalizeProficiency(proficiency);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeProficiency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Proficiency is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (!AllowedProficiencies.Contains(normalized))
        {
            throw new DomainException("Proficiency must be native, fluent, professional, intermediate, or basic.");
        }

        return normalized;
    }

    private static string NormalizeRequiredText(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }
}
