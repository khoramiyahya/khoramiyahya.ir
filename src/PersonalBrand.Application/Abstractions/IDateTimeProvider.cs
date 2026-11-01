namespace PersonalBrand.Application.Abstractions;

/// <summary>
/// Abstraction over system clock for testability and consistent UTC timestamps.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
