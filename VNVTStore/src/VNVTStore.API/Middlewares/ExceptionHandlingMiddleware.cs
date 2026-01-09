using System.Net;
using System.Text.Json;
using FluentValidation;
using VNVTStore.Application.Common;

namespace VNVTStore.API.Middlewares;

/// <summary>
/// Global Exception Handling Middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, _env);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized access"
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                exception.Message
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                exception.Message
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                exception.Message
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                env.IsDevelopment() 
                    ? $"{exception.Message} | Inner: {exception.InnerException?.Message}" 
                    : "An internal server error occurred"
            )
        };

        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(ApiResponse<string>.Fail(message, (int)statusCode));
        await response.WriteAsync(result);
    }
}

/// <summary>
/// Extension method để thêm middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
