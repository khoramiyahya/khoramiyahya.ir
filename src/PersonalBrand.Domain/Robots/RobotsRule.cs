using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Robots;

/// <summary>
/// A robots.txt allow or disallow rule owned by a user account.
/// </summary>
public sealed class RobotsRule : AggregateRoot<Guid>
{
    private static readonly HashSet<string> AllowedDirectives = new(StringComparer.OrdinalIgnoreCase)
    {
        "Allow",
        "Disallow"
    };

    private RobotsRule()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string UserAgent { get; private set; } = "*";

    public string Directive { get; private set; } = "Allow";

    public string Path { get; private set; } = "/";

    public int DisplayOrder { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static RobotsRule Create(
        string userId,
        string userAgent,
        string directive,
        string path,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Robots rule owner is required.");
        }

        var rule = new RobotsRule
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            DisplayOrder = 0,
            IsPublished = false
        };

        rule.ApplyContent(userAgent, directive, path);
        return rule;
    }

    public void UpdateDetails(
        string userAgent,
        string directive,
        string path,
        int displayOrder,
        DateTimeOffset utcNow)
    {
        ApplyContent(userAgent, directive, path);
        DisplayOrder = displayOrder < 0 ? 0 : displayOrder;
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(UserAgent)
            || string.IsNullOrWhiteSpace(Directive)
            || string.IsNullOrWhiteSpace(Path))
        {
            throw new DomainException("A published robots rule requires a user agent, directive, and path.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(string userAgent, string directive, string path)
    {
        UserAgent = NormalizeUserAgent(userAgent);
        Directive = NormalizeDirective(directive);
        Path = NormalizePath(path);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeUserAgent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "*";
        }

        var normalized = value.Trim();
        if (normalized.Length > 200)
        {
            throw new DomainException("User agent must be at most 200 characters.");
        }

        if (normalized.Contains('\n') || normalized.Contains('\r'))
        {
            throw new DomainException("User agent must be a single line.");
        }

        return normalized;
    }

    private static string NormalizeDirective(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Directive is required.");
        }

        var normalized = value.Trim();
        if (!AllowedDirectives.Contains(normalized))
        {
            throw new DomainException("Directive must be Allow or Disallow.");
        }

        return char.ToUpperInvariant(normalized[0]) + normalized[1..].ToLowerInvariant();
    }

    private static string NormalizePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Path is required.");
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        if (normalized.Length > 500)
        {
            throw new DomainException("Path must be at most 500 characters.");
        }

        if (normalized.Contains('\n') || normalized.Contains('\r'))
        {
            throw new DomainException("Path must be a single line.");
        }

        return normalized;
    }
}
