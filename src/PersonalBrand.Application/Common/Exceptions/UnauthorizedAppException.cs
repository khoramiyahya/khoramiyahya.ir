namespace PersonalBrand.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails or the caller is not authorized for the requested action.
/// </summary>
public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Unauthorized.")
        : base(message)
    {
    }
}
