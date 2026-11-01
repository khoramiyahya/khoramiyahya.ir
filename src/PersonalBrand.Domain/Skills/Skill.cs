using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Skills;

/// <summary>
/// Skill entry owned by a user account for the public brand site.
/// </summary>
public sealed class Skill : AggregateRoot<Guid>
{
    private Skill()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? Description { get; private set; }

    public int ProficiencyPercent { get; private set; }

    public int? YearsExperience { get; private set; }

    public string? IconName { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Skill Create(
        string userId,
        string name,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Skill owner is required.");
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            ProficiencyPercent = 0,
            IsPublished = false,
            IsFeatured = false
        };

        skill.ApplyName(name);
        return skill;
    }

    public void UpdateDetails(
        string name,
        string? category,
        string? description,
        int proficiencyPercent,
        int? yearsExperience,
        string? iconName,
        int displayOrder,
        bool isFeatured,
        DateTimeOffset utcNow)
    {
        ApplyName(name);
        Category = NormalizeOptionalText(category, "Category", 100);
        Description = NormalizeOptionalText(description, "Description", 1000);
        ProficiencyPercent = NormalizeProficiency(proficiencyPercent);
        YearsExperience = NormalizeYears(yearsExperience);
        IconName = NormalizeOptionalText(iconName, "Icon name", 100);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        IsFeatured = isFeatured;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new DomainException("A published skill requires a name.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyName(string name) =>
        Name = NormalizeRequiredText(name, "Name", 100);

    private static int NormalizeProficiency(int proficiencyPercent)
    {
        if (proficiencyPercent is < 0 or > 100)
        {
            throw new DomainException("Proficiency percent must be between 0 and 100.");
        }

        return proficiencyPercent;
    }

    private static int? NormalizeYears(int? yearsExperience)
    {
        if (yearsExperience is null)
        {
            return null;
        }

        if (yearsExperience < 0 || yearsExperience > 60)
        {
            throw new DomainException("Years of experience must be between 0 and 60.");
        }

        return yearsExperience;
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

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

    private static string? NormalizeOptionalText(string? value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must be at most {maxLength} characters.");
        }

        return normalized;
    }
}
