namespace PersonalBrand.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request conflicts with the current resource state.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
