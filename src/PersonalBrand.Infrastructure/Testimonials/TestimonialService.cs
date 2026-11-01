using FluentValidation;

using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Common.Validation;
using PersonalBrand.Application.Testimonials;
using PersonalBrand.Application.Testimonials.Contracts;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.Domain.Testimonials;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Testimonials;

internal sealed class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuditLogger _auditLogger;
    private readonly IValidator<CreateTestimonialRequest> _createValidator;
    private readonly IValidator<UpdateTestimonialRequest> _updateValidator;

    public TestimonialService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IAuditLogger auditLogger,
        IValidator<CreateTestimonialRequest> createValidator,
        IValidator<UpdateTestimonialRequest> updateValidator)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _auditLogger = auditLogger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<TestimonialDto>> GetPublishedAsync(
        CancellationToken cancellationToken = default)
    {
        var testimonials = await _dbContext.Testimonials
            .AsNoTracking()
            .Where(testimonial => testimonial.IsPublished)
            .OrderByDescending(testimonial => testimonial.IsFeatured)
            .ThenBy(testimonial => testimonial.DisplayOrder)
            .ThenByDescending(testimonial => testimonial.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return testimonials.Select(Map).ToArray();
    }

    public async Task<TestimonialDto> GetPublishedByIdAsync(
        Guid testimonialId,
        CancellationToken cancellationToken = default)
    {
        var testimonial = await _dbContext.Testimonials
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == testimonialId && item.IsPublished,
                cancellationToken);

        if (testimonial is null)
        {
            throw new NotFoundException("Testimonial", testimonialId);
        }

        return Map(testimonial);
    }

    public async Task<IReadOnlyList<TestimonialDto>> GetMineAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var testimonials = await _dbContext.Testimonials
            .AsNoTracking()
            .Where(testimonial => testimonial.UserId == userId)
            .OrderBy(testimonial => testimonial.DisplayOrder)
            .ThenByDescending(testimonial => testimonial.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return testimonials.Select(Map).ToArray();
    }

    public async Task<TestimonialDto> CreateMineAsync(
        CreateTestimonialRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var utcNow = _dateTimeProvider.UtcNow;

        Testimonial testimonial;
        try
        {
            testimonial = Testimonial.Create(userId, request.AuthorName, request.Quote, utcNow);
            testimonial.UpdateDetails(
                authorName: request.AuthorName,
                quote: request.Quote,
                authorRole: request.AuthorRole,
                company: request.Company,
                rating: request.Rating,
                authorImageUrl: request.AuthorImageUrl,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished)
            {
                testimonial.Publish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        _dbContext.Testimonials.Add(testimonial);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "testimonial.create",
            entityName: nameof(Testimonial),
            entityId: testimonial.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(testimonial);
    }

    public async Task<TestimonialDto> UpdateMineAsync(
        Guid testimonialId,
        UpdateTestimonialRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.EnsureValidAsync(request, cancellationToken);

        var userId = RequireCurrentUserId();
        var testimonial = await GetOwnedAsync(userId, testimonialId, cancellationToken);
        var utcNow = _dateTimeProvider.UtcNow;

        try
        {
            testimonial.UpdateDetails(
                authorName: request.AuthorName,
                quote: request.Quote,
                authorRole: request.AuthorRole,
                company: request.Company,
                rating: request.Rating,
                authorImageUrl: request.AuthorImageUrl,
                displayOrder: request.DisplayOrder,
                isFeatured: request.IsFeatured,
                utcNow: utcNow);

            if (request.IsPublished is true)
            {
                testimonial.Publish(utcNow);
            }
            else if (request.IsPublished is false)
            {
                testimonial.Unpublish(utcNow);
            }
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "testimonial.update",
            entityName: nameof(Testimonial),
            entityId: testimonial.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(testimonial);
    }

    public async Task DeleteMineAsync(Guid testimonialId, CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var testimonial = await GetOwnedAsync(userId, testimonialId, cancellationToken);

        _dbContext.Testimonials.Remove(testimonial);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "testimonial.delete",
            entityName: nameof(Testimonial),
            entityId: testimonial.Id.ToString(),
            cancellationToken: cancellationToken);
    }

    public async Task<TestimonialDto> PublishMineAsync(
        Guid testimonialId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var testimonial = await GetOwnedAsync(userId, testimonialId, cancellationToken);

        try
        {
            testimonial.Publish(_dateTimeProvider.UtcNow);
        }
        catch (DomainException exception)
        {
            throw ToValidationException(exception);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "testimonial.publish",
            entityName: nameof(Testimonial),
            entityId: testimonial.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(testimonial);
    }

    public async Task<TestimonialDto> UnpublishMineAsync(
        Guid testimonialId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();
        var testimonial = await GetOwnedAsync(userId, testimonialId, cancellationToken);

        testimonial.Unpublish(_dateTimeProvider.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogger.LogAsync(
            action: "testimonial.unpublish",
            entityName: nameof(Testimonial),
            entityId: testimonial.Id.ToString(),
            cancellationToken: cancellationToken);

        return Map(testimonial);
    }

    private async Task<Testimonial> GetOwnedAsync(
        string userId,
        Guid testimonialId,
        CancellationToken cancellationToken)
    {
        var testimonial = await _dbContext.Testimonials
            .SingleOrDefaultAsync(
                item => item.Id == testimonialId && item.UserId == userId,
                cancellationToken);

        if (testimonial is null)
        {
            throw new NotFoundException("Testimonial", testimonialId);
        }

        return testimonial;
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }

    private static TestimonialDto Map(Testimonial testimonial) =>
        new()
        {
            Id = testimonial.Id,
            AuthorName = testimonial.AuthorName,
            AuthorRole = testimonial.AuthorRole,
            Company = testimonial.Company,
            Quote = testimonial.Quote,
            Rating = testimonial.Rating,
            AuthorImageUrl = testimonial.AuthorImageUrl,
            DisplayOrder = testimonial.DisplayOrder,
            IsFeatured = testimonial.IsFeatured,
            IsPublished = testimonial.IsPublished,
            UpdatedAtUtc = testimonial.UpdatedAtUtc
        };

    private static AppValidationException ToValidationException(DomainException exception) =>
        new(new Dictionary<string, string[]>
        {
            ["Testimonial"] = [exception.Message]
        });
}
