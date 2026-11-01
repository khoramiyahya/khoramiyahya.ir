namespace PersonalBrand.Application.Dashboard.Contracts;

/// <summary>
/// Aggregated owner dashboard counters for the personal brand platform.
/// </summary>
public sealed class DashboardSummaryDto
{
    public required int ProjectsTotal { get; init; }

    public required int ProjectsPublished { get; init; }

    public required int BlogPostsTotal { get; init; }

    public required int BlogPostsPublished { get; init; }

    public required int ServicesTotal { get; init; }

    public required int ServicesPublished { get; init; }

    public required int TestimonialsTotal { get; init; }

    public required int TestimonialsPublished { get; init; }

    public required int SkillsTotal { get; init; }

    public required int SkillsPublished { get; init; }

    public required int ExperienceTotal { get; init; }

    public required int ExperiencePublished { get; init; }

    public required int EducationTotal { get; init; }

    public required int EducationPublished { get; init; }

    public required int FaqTotal { get; init; }

    public required int FaqPublished { get; init; }

    public required int CertificationsTotal { get; init; }

    public required int CertificationsPublished { get; init; }

    public required int ContactMessagesTotal { get; init; }

    public required int ContactMessagesUnread { get; init; }

    public required int NewsletterSubscribersActive { get; init; }

    public required int NewsletterSubscribersTotal { get; init; }

    public required int MediaAssetsTotal { get; init; }

    public required int PagesTotal { get; init; }

    public required int PagesPublished { get; init; }

    public required int SocialLinksTotal { get; init; }

    public required int SocialLinksPublished { get; init; }

    public required bool HasPublishedProfile { get; init; }

    public required DateTimeOffset GeneratedAtUtc { get; init; }
}
