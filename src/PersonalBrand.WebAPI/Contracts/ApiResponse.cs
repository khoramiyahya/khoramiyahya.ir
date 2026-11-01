namespace PersonalBrand.WebAPI.Contracts;

/// <summary>
/// Consistent API envelope for successful and failed responses.
/// </summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? Message { get; init; }

    public IDictionary<string, string[]>? Errors { get; init; }

    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            TraceId = traceId
        };

    public static ApiResponse<T> Fail(
        string message,
        IDictionary<string, string[]>? errors = null,
        string? traceId = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
}
