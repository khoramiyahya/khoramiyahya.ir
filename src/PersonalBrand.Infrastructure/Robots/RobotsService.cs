using System.Text;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Robots;
using PersonalBrand.Application.Robots.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Robots;
using PersonalBrand.Domain.Settings;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Robots;

internal sealed class RobotsService : IRobotsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateRobotsRuleRequest> _createValidator;
    private readonly IValidator<UpdateRobotsRuleRequest> _updateValidator;

    public RobotsService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateRobotsRuleRequest> createValidator,
        IValidator<UpdateRobotsRuleRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<string> GetPublicTextAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _dbContext.RobotsRules
            .AsNoTracking()
            .Where(rule => rule.IsPublished)
            .OrderBy(rule => rule.UserAgent)
            .ThenBy(rule => rule.DisplayOrder)
            .ThenBy(rule => rule.Path)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();

        if (rules.Count == 0)
        {
            builder.AppendLine("User-agent: *");
            builder.AppendLine("Allow: /");
            builder.AppendLine("Disallow: /api/");
        }
        else
        {
            string? currentAgent = null;
            foreach (var rule in rules)
            {
                if (!string.Equals(currentAgent, rule.UserAgent, StringComparison.Ordinal))
                {
                    if (currentAgent is not null)
                    {
                        builder.AppendLine();
                    }

                    builder.Append("User-agent: ").AppendLine(rule.UserAgent);
                    currentAgent = rule.UserAgent;
                }

                builder.Append(rule.Directive).Append(": ").AppendLine(rule.Path);
            }
        }

        var sitemapUrl = await BuildSitemapUrlAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(sitemapUrl))
        {
            builder.AppendLine();
            builder.Append("Sitemap: ").AppendLine(sitemapUrl);
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    public async Task<IReadOnlyList<RobotsRuleDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var rules = await _dbContext.RobotsRules
            .AsNoTracking()
            .Where(rule => rule.UserId == userId)
            .OrderBy(rule => rule.DisplayOrder)
            .ThenBy(rule => rule.Path)
            .ToListAsync(cancellationToken);

        return rules.Select(Map).ToArray();
    }

    public async Task<RobotsRuleDto> CreateMineAsync(
        CreateRobotsRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        RobotsRule rule;
        try
        {
            rule = RobotsRule.Create(
                userId,
                request.UserAgent,
                request.Directive,
                request.Path,
                utcNow);
            rule.UpdateDetails(
                userAgent: request.UserAgent,
                directive: request.Directive,
                path: request.Path,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                rule.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.RobotsRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "robots.create",
            entityName: nameof(RobotsRule),
            entityId: rule.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(rule);
    }

    public async Task<RobotsRuleDto> UpdateMineAsync(
        Guid robotsRuleId,
        UpdateRobotsRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var rule = await GetOwnedAsync(userId, robotsRuleId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            rule.UpdateDetails(
                userAgent: request.UserAgent,
                directive: request.Directive,
                path: request.Path,
                displayOrder: request.DisplayOrder,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                rule.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                rule.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "robots.update",
            entityName: nameof(RobotsRule),
            entityId: rule.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(rule);
    }

    public async Task DeleteMineAsync(Guid robotsRuleId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var rule = await GetOwnedAsync(userId, robotsRuleId, cancellationToken);

        _dbContext.RobotsRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "robots.delete",
            entityName: nameof(RobotsRule),
            entityId: rule.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<RobotsRuleDto> PublishMineAsync(
        Guid robotsRuleId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var rule = await GetOwnedAsync(userId, robotsRuleId, cancellationToken);

        try
        {
            rule.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "robots.publish",
            entityName: nameof(RobotsRule),
            entityId: rule.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(rule);
    }

    public async Task<RobotsRuleDto> UnpublishMineAsync(
        Guid robotsRuleId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var rule = await GetOwnedAsync(userId, robotsRuleId, cancellationToken);

        rule.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "robots.unpublish",
            entityName: nameof(RobotsRule),
            entityId: rule.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(rule);
    }

    private async Task<string?> BuildSitemapUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SiteSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Key == SiteSettings.SingletonKey, cancellationToken);

        if (string.IsNullOrWhiteSpace(settings?.CanonicalBaseUrl))
        {
            return "/api/v1/sitemap/xml";
        }

        return settings.CanonicalBaseUrl.TrimEnd('/') + "/api/v1/sitemap/xml";
    }

    private async Task<RobotsRule> GetOwnedAsync(
        string userId,
        Guid robotsRuleId,
        CancellationToken cancellationToken)
    {
        var rule = await _dbContext.RobotsRules
            .SingleOrDefaultAsync(
                entry => entry.Id == robotsRuleId && entry.UserId == userId,
                cancellationToken);

        if (rule is null)
        {
            throw new NotFoundException("RobotsRule", robotsRuleId);
        }

        return rule;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static RobotsRuleDto Map(RobotsRule rule) =>
        new()
        {
            Id = rule.Id,
            UserAgent = rule.UserAgent,
            Directive = rule.Directive,
            Path = rule.Path,
            DisplayOrder = rule.DisplayOrder,
            IsPublished = rule.IsPublished,
            UpdatedAtUtc = rule.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Robots"] = [exception.Message]
        });
}
