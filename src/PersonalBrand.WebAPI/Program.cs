using Asp.Versioning;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using PersonalBrand.Application;
using PersonalBrand.Application.Abstractions;
using PersonalBrand.Infrastructure;
using PersonalBrand.Infrastructure.Identity;
using PersonalBrand.Infrastructure.Persistence;
using PersonalBrand.WebAPI.Extensions;
using PersonalBrand.WebAPI.Middleware;
using PersonalBrand.WebAPI.Services;

using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "PersonalBrand.WebAPI")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/personal-brand-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14),
        preserveStaticLogger: true);

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
    builder.Services.AddConfiguredCors();
    builder.Services.AddConfiguredRateLimiting(builder.Configuration);
    builder.Services.AddConfiguredSwagger();

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>(
            name: "database",
            failureStatus: HealthStatus.Unhealthy);

    var app = builder.Build();

    await InitializeDatabaseAsync(app);

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    app.UseConfiguredSwagger();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseConfiguredCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers()
        .RequireRateLimiting(RateLimitingExtensions.GlobalPolicyName);

    app.MapHealthChecks("/health")
        .AllowAnonymous()
        .DisableRateLimiting();

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var applyMigrations =
        app.Environment.IsEnvironment("Testing")
        || app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment());

    if (applyMigrations)
    {
        // MigrateAsync creates the database when it does not exist, then applies pending migrations.
        await dbContext.Database.MigrateAsync();
    }

    await IdentityDataSeeder.SeedAsync(app.Services);
}

/// <summary>
/// Exposes the entry point assembly for integration tests.
/// </summary>
public partial class Program;
