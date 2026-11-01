using Microsoft.EntityFrameworkCore;

using PersonalBrand.Application.Abstractions;
using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Application.Dashboard;
using PersonalBrand.Application.Dashboard.Contracts;
using PersonalBrand.Infrastructure.Persistence;

namespace PersonalBrand.Infrastructure.Dashboard;

internal sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DashboardService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireCurrentUserId();

        var projectsTotal = await _dbContext.PortfolioProjects.CountAsync(item => item.UserId == userId, cancellationToken);
        var projectsPublished = await _dbContext.PortfolioProjects.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var blogTotal = await _dbContext.BlogPosts.CountAsync(item => item.UserId == userId, cancellationToken);
        var blogPublished = await _dbContext.BlogPosts.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var servicesTotal = await _dbContext.ServiceOffers.CountAsync(item => item.UserId == userId, cancellationToken);
        var servicesPublished = await _dbContext.ServiceOffers.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var testimonialsTotal = await _dbContext.Testimonials.CountAsync(item => item.UserId == userId, cancellationToken);
        var testimonialsPublished = await _dbContext.Testimonials.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var skillsTotal = await _dbContext.Skills.CountAsync(item => item.UserId == userId, cancellationToken);
        var skillsPublished = await _dbContext.Skills.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var experienceTotal = await _dbContext.WorkExperiences.CountAsync(item => item.UserId == userId, cancellationToken);
        var experiencePublished = await _dbContext.WorkExperiences.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var educationTotal = await _dbContext.EducationEntries.CountAsync(item => item.UserId == userId, cancellationToken);
        var educationPublished = await _dbContext.EducationEntries.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var faqTotal = await _dbContext.FaqItems.CountAsync(item => item.UserId == userId, cancellationToken);
        var faqPublished = await _dbContext.FaqItems.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var certificationsTotal = await _dbContext.Certifications.CountAsync(item => item.UserId == userId, cancellationToken);
        var certificationsPublished = await _dbContext.Certifications.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var contactTotal = await _dbContext.ContactMessages.CountAsync(cancellationToken);
        var contactUnread = await _dbContext.ContactMessages.CountAsync(item => !item.IsRead, cancellationToken);

        var newsletterActive = await _dbContext.NewsletterSubscribers.CountAsync(item => item.IsActive, cancellationToken);
        var newsletterTotal = await _dbContext.NewsletterSubscribers.CountAsync(cancellationToken);

        var mediaTotal = await _dbContext.MediaAssets.CountAsync(item => item.UserId == userId, cancellationToken);

        var pagesTotal = await _dbContext.ContentPages.CountAsync(item => item.UserId == userId, cancellationToken);
        var pagesPublished = await _dbContext.ContentPages.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var socialLinksTotal = await _dbContext.SocialLinks.CountAsync(item => item.UserId == userId, cancellationToken);
        var socialLinksPublished = await _dbContext.SocialLinks.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var navigationTotal = await _dbContext.NavigationItems.CountAsync(item => item.UserId == userId, cancellationToken);
        var navigationPublished = await _dbContext.NavigationItems.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var redirectsTotal = await _dbContext.SeoRedirects.CountAsync(item => item.UserId == userId, cancellationToken);
        var redirectsPublished = await _dbContext.SeoRedirects.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var robotsTotal = await _dbContext.RobotsRules.CountAsync(item => item.UserId == userId, cancellationToken);
        var robotsPublished = await _dbContext.RobotsRules.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var partnersTotal = await _dbContext.Partners.CountAsync(item => item.UserId == userId, cancellationToken);
        var partnersPublished = await _dbContext.Partners.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var awardsTotal = await _dbContext.Awards.CountAsync(item => item.UserId == userId, cancellationToken);
        var awardsPublished = await _dbContext.Awards.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var talksTotal = await _dbContext.Talks.CountAsync(item => item.UserId == userId, cancellationToken);
        var talksPublished = await _dbContext.Talks.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var publicationsTotal = await _dbContext.Publications.CountAsync(item => item.UserId == userId, cancellationToken);
        var publicationsPublished = await _dbContext.Publications.CountAsync(
            item => item.UserId == userId && item.IsPublished,
            cancellationToken);

        var hasPublishedProfile = await _dbContext.BrandProfiles.AnyAsync(
            profile => profile.UserId == userId && profile.IsPublished,
            cancellationToken);

        return new DashboardSummaryDto
        {
            ProjectsTotal = projectsTotal,
            ProjectsPublished = projectsPublished,
            BlogPostsTotal = blogTotal,
            BlogPostsPublished = blogPublished,
            ServicesTotal = servicesTotal,
            ServicesPublished = servicesPublished,
            TestimonialsTotal = testimonialsTotal,
            TestimonialsPublished = testimonialsPublished,
            SkillsTotal = skillsTotal,
            SkillsPublished = skillsPublished,
            ExperienceTotal = experienceTotal,
            ExperiencePublished = experiencePublished,
            EducationTotal = educationTotal,
            EducationPublished = educationPublished,
            FaqTotal = faqTotal,
            FaqPublished = faqPublished,
            CertificationsTotal = certificationsTotal,
            CertificationsPublished = certificationsPublished,
            ContactMessagesTotal = contactTotal,
            ContactMessagesUnread = contactUnread,
            NewsletterSubscribersActive = newsletterActive,
            NewsletterSubscribersTotal = newsletterTotal,
            MediaAssetsTotal = mediaTotal,
            PagesTotal = pagesTotal,
            PagesPublished = pagesPublished,
            SocialLinksTotal = socialLinksTotal,
            SocialLinksPublished = socialLinksPublished,
            NavigationItemsTotal = navigationTotal,
            NavigationItemsPublished = navigationPublished,
            RedirectsTotal = redirectsTotal,
            RedirectsPublished = redirectsPublished,
            RobotsRulesTotal = robotsTotal,
            RobotsRulesPublished = robotsPublished,
            PartnersTotal = partnersTotal,
            PartnersPublished = partnersPublished,
            AwardsTotal = awardsTotal,
            AwardsPublished = awardsPublished,
            TalksTotal = talksTotal,
            TalksPublished = talksPublished,
            PublicationsTotal = publicationsTotal,
            PublicationsPublished = publicationsPublished,
            HasPublishedProfile = hasPublishedProfile,
            GeneratedAtUtc = _dateTimeProvider.UtcNow
        };
    }

    private string RequireCurrentUserId()
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.UserId))
        {
            throw new UnauthorizedAppException();
        }

        return _currentUserService.UserId;
    }
}
