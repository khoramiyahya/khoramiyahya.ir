using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Schema;
using PersonalBrand.Domain.Settings;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Schema;

internal sealed class SchemaService : ISchemaService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ApplicationDbContext _dbContext;

    public SchemaService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetPublicJsonLdAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.SiteSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Key == SiteSettings.SingletonKey, cancellationToken);

        var baseUrl = string.IsNullOrWhiteSpace(settings?.CanonicalBaseUrl)
            ? null
            : settings.CanonicalBaseUrl.TrimEnd('/');

        var profiles = await _dbContext.BrandProfiles
            .AsNoTracking()
            .Where(profile => profile.IsPublished)
            .OrderBy(profile => profile.DisplayName)
            .ToListAsync(cancellationToken);

        var socialLinks = await _dbContext.SocialLinks
            .AsNoTracking()
            .Where(link => link.IsPublished)
            .Select(link => new { link.UserId, link.Url })
            .ToListAsync(cancellationToken);

        var skills = await _dbContext.Skills
            .AsNoTracking()
            .Where(skill => skill.IsPublished)
            .OrderBy(skill => skill.DisplayOrder)
            .Select(skill => new { skill.UserId, skill.Name })
            .ToListAsync(cancellationToken);

        var graph = new JsonArray();

        var website = new JsonObject
        {
            ["@type"] = "WebSite",
            ["name"] = string.IsNullOrWhiteSpace(settings?.SiteName) ? "Personal Brand" : settings.SiteName
        };

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            website["url"] = baseUrl + "/";
            website["@id"] = baseUrl + "/#website";
        }

        if (!string.IsNullOrWhiteSpace(settings?.MetaDescription))
        {
            website["description"] = settings.MetaDescription;
        }

        graph.Add(website);

        foreach (var profile in profiles)
        {
            var person = new JsonObject
            {
                ["@type"] = "Person",
                ["name"] = profile.DisplayName
            };

            var profileUrl = baseUrl is null
                ? $"/u/{profile.Slug}"
                : $"{baseUrl}/u/{profile.Slug}";
            person["url"] = string.IsNullOrWhiteSpace(profile.WebsiteUrl) ? profileUrl : profile.WebsiteUrl;
            person["@id"] = profileUrl + "#person";

            if (!string.IsNullOrWhiteSpace(profile.Headline))
            {
                person["jobTitle"] = profile.Headline;
            }

            if (!string.IsNullOrWhiteSpace(profile.Bio))
            {
                person["description"] = profile.Bio;
            }

            if (!string.IsNullOrWhiteSpace(profile.Location))
            {
                person["homeLocation"] = profile.Location;
            }

            if (!string.IsNullOrWhiteSpace(settings?.PrimaryEmail))
            {
                person["email"] = settings.PrimaryEmail;
            }

            var sameAs = new JsonArray();
            AddUrl(sameAs, profile.WebsiteUrl);
            AddUrl(sameAs, profile.LinkedInUrl);
            AddUrl(sameAs, profile.GitHubUrl);
            AddUrl(sameAs, profile.TwitterUrl);
            AddUrl(sameAs, profile.InstagramUrl);

            foreach (var link in socialLinks.Where(item => item.UserId == profile.UserId))
            {
                AddUrl(sameAs, link.Url);
            }

            if (sameAs.Count > 0)
            {
                person["sameAs"] = sameAs;
            }

            var knowsAbout = new JsonArray();
            foreach (var skill in skills.Where(item => item.UserId == profile.UserId))
            {
                if (!string.IsNullOrWhiteSpace(skill.Name))
                {
                    knowsAbout.Add(skill.Name);
                }
            }

            if (knowsAbout.Count > 0)
            {
                person["knowsAbout"] = knowsAbout;
            }

            graph.Add(person);
        }

        var document = new JsonObject
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph
        };

        return document.ToJsonString(JsonOptions);
    }

    private static void AddUrl(JsonArray target, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        foreach (var existing in target)
        {
            if (string.Equals(existing?.GetValue<string>(), url, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        target.Add(url);
    }
}
