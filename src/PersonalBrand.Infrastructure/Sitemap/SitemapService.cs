using System.Globalization;
using System.Text;
using System.Xml.Linq;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Sitemap;
using PersonalBrand.Application.Sitemap.Contracts;
using PersonalBrand.Domain.Settings;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Sitemap;

internal sealed class SitemapService : ISitemapService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SitemapService(
        ApplicationDbContext dbContext,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<SitemapDto> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        var entries = await BuildEntriesAsync(cancellationToken);
        var baseUrl = await GetCanonicalBaseUrlAsync(cancellationToken);

        return new SitemapDto
        {
            BaseUrl = baseUrl,
            GeneratedAtUtc = _dateTimeProvider.UtcNow,
            TotalCount = entries.Count,
            Entries = entries
        };
    }

    public async Task<string> GetPublicXmlAsync(CancellationToken cancellationToken = default)
    {
        var sitemap = await GetPublicAsync(cancellationToken);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(
                ns + "urlset",
                sitemap.Entries.Select(entry =>
                    new XElement(
                        ns + "url",
                        new XElement(ns + "loc", entry.Loc),
                        new XElement(
                            ns + "lastmod",
                            entry.LastModifiedUtc.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                        new XElement(ns + "changefreq", entry.ChangeFrequency),
                        new XElement(
                            ns + "priority",
                            entry.Priority.ToString("0.0", CultureInfo.InvariantCulture))))));

        var builder = new StringBuilder();
        await using var writer = new StringWriter(builder);
        document.Save(writer);
        return builder.ToString();
    }

    private async Task<IReadOnlyList<SitemapEntryDto>> BuildEntriesAsync(CancellationToken cancellationToken)
    {
        var baseUrl = await GetCanonicalBaseUrlAsync(cancellationToken);
        var entries = new List<SitemapEntryDto>();

        void Add(string path, DateTimeOffset lastModifiedUtc, string changeFrequency, decimal priority, string contentType)
        {
            entries.Add(new SitemapEntryDto
            {
                Loc = CombineUrl(baseUrl, path),
                LastModifiedUtc = lastModifiedUtc,
                ChangeFrequency = changeFrequency,
                Priority = priority,
                ContentType = contentType
            });
        }

        Add("/", _dateTimeProvider.UtcNow, "daily", 1.0m, "home");

        var profiles = await _dbContext.BrandProfiles
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .Select(item => new { item.Slug, item.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var profile in profiles)
        {
            Add($"/u/{profile.Slug}", profile.UpdatedAtUtc, "weekly", 0.9m, "profile");
        }

        var projects = await _dbContext.PortfolioProjects
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .Select(item => new { item.Slug, item.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var project in projects)
        {
            Add($"/projects/{project.Slug}", project.UpdatedAtUtc, "weekly", 0.8m, "project");
        }

        var posts = await _dbContext.BlogPosts
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .Select(item => new { item.Slug, item.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var post in posts)
        {
            Add($"/blog/{post.Slug}", post.UpdatedAtUtc, "weekly", 0.8m, "blog");
        }

        var services = await _dbContext.ServiceOffers
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .Select(item => new { item.Slug, item.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var service in services)
        {
            Add($"/services/{service.Slug}", service.UpdatedAtUtc, "monthly", 0.7m, "service");
        }

        var pages = await _dbContext.ContentPages
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .Select(item => new { item.Slug, item.UpdatedAtUtc })
            .ToListAsync(cancellationToken);

        foreach (var page in pages)
        {
            Add($"/pages/{page.Slug}", page.UpdatedAtUtc, "monthly", 0.6m, "page");
        }

        return entries
            .OrderByDescending(entry => entry.Priority)
            .ThenBy(entry => entry.Loc)
            .ToArray();
    }

    private async Task<string?> GetCanonicalBaseUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SiteSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Key == SiteSettings.SingletonKey, cancellationToken);

        return string.IsNullOrWhiteSpace(settings?.CanonicalBaseUrl)
            ? null
            : settings.CanonicalBaseUrl.TrimEnd('/');
    }

    private static string CombineUrl(string? baseUrl, string path)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return path.StartsWith('/') ? path : "/" + path;
        }

        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            return baseUrl + "/";
        }

        return baseUrl + (path.StartsWith('/') ? path : "/" + path);
    }
}
