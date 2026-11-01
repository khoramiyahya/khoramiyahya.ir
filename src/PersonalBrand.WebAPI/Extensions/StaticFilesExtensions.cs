using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

using PersonalBrand.Infrastructure.Options;

namespace PersonalBrand.WebAPI.Extensions;

internal static class StaticFilesExtensions
{
    public static IApplicationBuilder UseUploadStaticFiles(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<FileStorageOptions>>().Value;
        var rootPath = Path.IsPathRooted(options.RootPath)
            ? options.RootPath
            : Path.Combine(app.Environment.ContentRootPath, options.RootPath);

        Directory.CreateDirectory(rootPath);

        var requestPath = string.IsNullOrWhiteSpace(options.RequestPath)
            ? "/uploads"
            : options.RequestPath;

        if (!requestPath.StartsWith('/'))
        {
            requestPath = "/" + requestPath;
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPath),
            RequestPath = requestPath
        });

        return app;
    }
}
