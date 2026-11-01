namespace PersonalBrand.Application.Abstractions;

/// <summary>
/// Abstraction for storing and retrieving files. Shared hosting constraints apply at the Infrastructure implementation.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
