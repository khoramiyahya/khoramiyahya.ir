namespace PersonalBrand.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested resource cannot be found.
/// </summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }
}
