using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Profiles;

/// <summary>
/// Helpers for creating and validating public profile slugs.
/// </summary>
public static partial class ProfileSlug
{
    private static readonly Regex InvalidCharactersRegex = CreateInvalidCharactersRegex();

    public static string FromDisplayName(string displayName)
    {
        var source = displayName.Trim().ToLowerInvariant();
        source = RemoveDiacritics(source);
        source = InvalidCharactersRegex.Replace(source, "-");
        source = CollapseHyphensRegex().Replace(source, "-").Trim('-');

        if (string.IsNullOrWhiteSpace(source))
        {
            source = "profile";
        }

        return Normalize(source);
    }

    public static string Normalize(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Slug is required.");
        }

        var normalized = slug.Trim().ToLowerInvariant();
        normalized = RemoveDiacritics(normalized);
        normalized = InvalidCharactersRegex.Replace(normalized, "-");
        normalized = CollapseHyphensRegex().Replace(normalized, "-").Trim('-');

        if (normalized.Length is < 2 or > 100)
        {
            throw new DomainException("Slug must be between 2 and 100 characters.");
        }

        if (!SlugFormatRegex().IsMatch(normalized))
        {
            throw new DomainException("Slug may contain only lowercase letters, numbers, and hyphens.");
        }

        return normalized;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex("[^a-z0-9-]+")]
    private static partial Regex CreateInvalidCharactersRegex();

    [GeneratedRegex("-{2,}")]
    private static partial Regex CollapseHyphensRegex();

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugFormatRegex();
}
