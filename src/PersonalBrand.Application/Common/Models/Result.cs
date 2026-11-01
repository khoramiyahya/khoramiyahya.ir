namespace PersonalBrand.Application.Common.Models;

/// <summary>
/// Lightweight operation result without a payload.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("Successful results cannot contain an error.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException("Failed results must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

/// <summary>
/// Lightweight operation result with a payload.
/// </summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public new static Result<T> Failure(string error) => new(false, default, error);
}
