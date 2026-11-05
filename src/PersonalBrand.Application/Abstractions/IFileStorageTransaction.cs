namespace PersonalBrand.Application.Abstractions;

/// <summary>
/// Coordinates staged file mutations with the current application transaction.
/// </summary>
public interface IFileStorageTransaction
{
    void Begin();

    Task PrepareAsync(CancellationToken cancellationToken = default);

    Task CompleteAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task AbandonAsync(CancellationToken cancellationToken = default);
}
