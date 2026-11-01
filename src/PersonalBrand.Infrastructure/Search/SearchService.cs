using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Search;
using PersonalBrand.Application.Search.Contracts;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Search;

internal sealed class SearchService : ISearchService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<SearchRequest> _validator;

    public SearchService(
        ApplicationDbContext dbContext,
        IValidator<SearchRequest> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<SearchResponseDto> SearchPublishedAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.EnsureValidAsync(request, cancellationToken);

        var query = request.Query.Trim();
        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? null
            : request.ContentType.Trim().ToLowerInvariant();
        var limit = request.Limit;

        var results = new List<SearchResultItemDto>();

        if (ShouldInclude(contentType, "project"))
        {
            var projects = await _dbContext.PortfolioProjects
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || (item.Summary != null && item.Summary.Contains(query))
                    || (item.Description != null && item.Description.Contains(query))
                    || (item.Technologies != null && item.Technologies.Contains(query))
                    || item.Slug.Contains(query))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "project",
                    Title = item.Title,
                    Summary = item.Summary,
                    Slug = item.Slug,
                    UrlHint = $"/projects/{item.Slug}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(projects);
        }

        if (ShouldInclude(contentType, "blog"))
        {
            var posts = await _dbContext.BlogPosts
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || (item.Summary != null && item.Summary.Contains(query))
                    || (item.Body != null && item.Body.Contains(query))
                    || (item.Tags != null && item.Tags.Contains(query))
                    || item.Slug.Contains(query))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "blog",
                    Title = item.Title,
                    Summary = item.Summary,
                    Slug = item.Slug,
                    UrlHint = $"/blog/{item.Slug}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(posts);
        }

        if (ShouldInclude(contentType, "service"))
        {
            var services = await _dbContext.ServiceOffers
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || (item.Summary != null && item.Summary.Contains(query))
                    || (item.Description != null && item.Description.Contains(query))
                    || item.Slug.Contains(query))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "service",
                    Title = item.Title,
                    Summary = item.Summary,
                    Slug = item.Slug,
                    UrlHint = $"/services/{item.Slug}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(services);
        }

        if (ShouldInclude(contentType, "page"))
        {
            var pages = await _dbContext.ContentPages
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || (item.Summary != null && item.Summary.Contains(query))
                    || (item.Body != null && item.Body.Contains(query))
                    || item.Slug.Contains(query))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "page",
                    Title = item.Title,
                    Summary = item.Summary,
                    Slug = item.Slug,
                    UrlHint = $"/pages/{item.Slug}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(pages);
        }

        if (ShouldInclude(contentType, "faq"))
        {
            var faqs = await _dbContext.FaqItems
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Question.Contains(query)
                    || item.Answer.Contains(query)
                    || (item.Category != null && item.Category.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "faq",
                    Title = item.Question,
                    Summary = item.Answer.Length > 200 ? item.Answer.Substring(0, 200) : item.Answer,
                    Slug = null,
                    UrlHint = $"/faq/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(faqs);
        }

        if (ShouldInclude(contentType, "skill"))
        {
            var skills = await _dbContext.Skills
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Name.Contains(query)
                    || (item.Category != null && item.Category.Contains(query))
                    || (item.Description != null && item.Description.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "skill",
                    Title = item.Name,
                    Summary = item.Description ?? item.Category,
                    Slug = null,
                    UrlHint = $"/skills/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(skills);
        }

        if (ShouldInclude(contentType, "certification"))
        {
            var certifications = await _dbContext.Certifications
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Name.Contains(query)
                    || item.Issuer.Contains(query)
                    || (item.CredentialId != null && item.CredentialId.Contains(query))
                    || (item.Description != null && item.Description.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "certification",
                    Title = item.Name,
                    Summary = item.Issuer,
                    Slug = null,
                    UrlHint = $"/certifications/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(certifications);
        }

        if (ShouldInclude(contentType, "experience"))
        {
            var experiences = await _dbContext.WorkExperiences
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || item.Company.Contains(query)
                    || (item.Location != null && item.Location.Contains(query))
                    || (item.Description != null && item.Description.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "experience",
                    Title = item.Title,
                    Summary = item.Company,
                    Slug = null,
                    UrlHint = $"/experience/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(experiences);
        }

        if (ShouldInclude(contentType, "education"))
        {
            var education = await _dbContext.EducationEntries
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Institution.Contains(query)
                    || item.Degree.Contains(query)
                    || (item.FieldOfStudy != null && item.FieldOfStudy.Contains(query))
                    || (item.Description != null && item.Description.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "education",
                    Title = item.Degree,
                    Summary = item.Institution,
                    Slug = null,
                    UrlHint = $"/education/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(education);
        }

        if (ShouldInclude(contentType, "award"))
        {
            var awards = await _dbContext.Awards
                .AsNoTracking()
                .Where(item => item.IsPublished)
                .Where(item =>
                    item.Title.Contains(query)
                    || item.Issuer.Contains(query)
                    || (item.Description != null && item.Description.Contains(query)))
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(limit)
                .Select(item => new SearchResultItemDto
                {
                    Id = item.Id,
                    ContentType = "award",
                    Title = item.Title,
                    Summary = item.Issuer,
                    Slug = null,
                    UrlHint = $"/awards/{item.Id}",
                    UpdatedAtUtc = item.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

            results.AddRange(awards);
        }

        var ranked = results
            .OrderByDescending(item => item.UpdatedAtUtc)
            .Take(limit)
            .ToArray();

        return new SearchResponseDto
        {
            Query = query,
            TotalCount = ranked.Length,
            Items = ranked
        };
    }

    private static bool ShouldInclude(string? contentTypeFilter, string contentType) =>
        contentTypeFilter is null
        || string.Equals(contentTypeFilter, contentType, StringComparison.OrdinalIgnoreCase);
}
