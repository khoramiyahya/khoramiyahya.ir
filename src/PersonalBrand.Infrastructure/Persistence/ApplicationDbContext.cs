using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PersonalBrand.Domain.Blog;
using PersonalBrand.Domain.Contact;
using PersonalBrand.Domain.Profiles;
using PersonalBrand.Domain.Projects;
using PersonalBrand.Domain.Services;
using PersonalBrand.Domain.Testimonials;
using PersonalBrand.Infrastructure.Identity;

namespace PersonalBrand.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for Identity and future business modules.
/// </summary>
public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<BrandProfile> BrandProfiles => Set<BrandProfile>();

    public DbSet<PortfolioProject> PortfolioProjects => Set<PortfolioProject>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<ServiceOffer> ServiceOffers => Set<ServiceOffer>();

    public DbSet<Testimonial> Testimonials => Set<Testimonial>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
