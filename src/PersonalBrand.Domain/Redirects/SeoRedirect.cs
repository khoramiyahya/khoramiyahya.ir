using PersonalBrand.Domain.Common;
using PersonalBrand.Domain.Exceptions;

namespace PersonalBrand.Domain.Redirects;

/// <summary>
/// SEO redirect rule owned by a user account.
/// </summary>
public sealed class SeoRedirect : AggregateRoot<Guid>
{
    private SeoRedirect()
    {
    }

    public string UserId { get; private set; } = string.Empty;

    public string FromPath { get; private set; } = string.Empty;

    public string ToUrl { get; private set; } = string.Empty;

    public int StatusCode { get; private set; }

    public string? Notes { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SeoRedirect Create(
        string userId,
        string fromPath,
        string toUrl,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainException("Redirect owner is required.");
        }

        var redirect = new SeoRedirect
        {
            Id = Guid.NewGuid(),
            UserId = userId.Trim(),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            StatusCode = 301,
            IsPublished = false
        };

        redirect.ApplyContent(fromPath, toUrl, 301, null);
        return redirect;
    }

    public void UpdateDetails(
        string fromPath,
        string toUrl,
        int statusCode,
        string? notes,
        DateTimeOffset utcNow)
    {
        ApplyContent(fromPath, toUrl, statusCode, notes);
        Touch(utcNow);
    }

    public void Publish(DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(FromPath) || string.IsNullOrWhiteSpace(ToUrl))
        {
            throw new DomainException("A published redirect requires a from path and destination URL.");
        }

        IsPublished = true;
        Touch(utcNow);
    }

    public void Unpublish(DateTimeOffset utcNow)
    {
        IsPublished = false;
        Touch(utcNow);
    }

    private void ApplyContent(string fromPath, string toUrl, int statusCode, string? notes)
    {
        FromPath = NormalizeFromPath(fromPath);
        ToUrl = NormalizeToUrl(toUrl);
        StatusCode = NormalizeStatusCode(statusCode);
        Notes = NormalizeOptionalText(notes, "Notes", 500);
    }

    private void Touch(DateTimeOffset utcNow) => UpdatedAtUtc = utcNow;

    private static string NormalizeFromPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("From path is required.");
        }

        var normalized = value.Trim();
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        if (normalized.Length > 500)
        {
            throw new DomainException("From path must be at most 500 characters.");
        }

        if (Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new DomainException("From path must be a relative path starting with '/'.");
        }

        return normalized;
    }

    private static string NormalizeToUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Destination URL is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > 500)
        {
            throw new DomainException("Destination URL must be at most 500 characters.");
        }

        if (normalized.StartsWith('/') || normalized.StartsWith('#'))
        {
            return normalized;
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("Destination URL must be a relative path or a valid http/https URL.");
        }

        return normalized;
    }

    private static int NormalizeStatusCode(int statusCode)
    {
        if (statusCode is not (301 or 302 or 307 or 308))
        {
            throw new DomainException("Status code must be 301, 302, 307, or 308.");
        }

        return statusCode;
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
