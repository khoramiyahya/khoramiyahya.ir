using System.Net;
using System.Text.Json;

using PersonalBrand.Application.Common.Exceptions;
using PersonalBrand.Domain.Exceptions;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Middleware;

/// <summary>
/// Converts unhandled exceptions into consistent API responses without leaking internals in Production.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception. TraceId={TraceId}", traceId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errors, traceId);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }

    private (int StatusCode, string Message, IDictionary<string, string[]>? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            AppValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                validationException.Message,
                validationException.Errors),

            NotFoundException notFoundException => (
                (int)HttpStatusCode.NotFound,
                notFoundException.Message,
                null),

            ConflictException conflictException => (
                (int)HttpStatusCode.Conflict,
                conflictException.Message,
                null),

            UnauthorizedAppException unauthorizedAppException => (
                (int)HttpStatusCode.Unauthorized,
                unauthorizedAppException.Message,
                null),

            DomainException domainException => (
                (int)HttpStatusCode.Conflict,
                domainException.Message,
                null),

            AppException appException => (
                (int)HttpStatusCode.BadRequest,
                appException.Message,
                null),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized.",
                null),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                null)
        };
    }
}
