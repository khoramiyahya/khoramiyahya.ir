namespace PersonalBrand.Application.Common.Exceptions;

/// <summary>
/// Thrown when one or more validation failures occur.
/// </summary>
public sealed class AppValidationException : AppException
{
    public AppValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    public AppValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = new Dictionary<string, string[]>(errors, StringComparer.OrdinalIgnoreCase);
    }

    public IDictionary<string, string[]> Errors { get; }
}
