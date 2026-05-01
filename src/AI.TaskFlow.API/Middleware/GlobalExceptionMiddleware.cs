using System.Net;
using System.Text.Json;
using AI.TaskFlow.Application.Common;

namespace AI.TaskFlow.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");

            context.Response.StatusCode = exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };
            context.Response.ContentType = "application/json";

            var message = exception switch
            {
                ArgumentException => exception.Message,
                ForbiddenAccessException => exception.Message,
                InvalidOperationException => exception.Message,
                KeyNotFoundException => exception.Message,
                UnauthorizedAccessException => exception.Message,
                _ => "An unexpected error occurred."
            };

            var response = ApiResponse<object>.Failure(message, context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
                ? "Please contact support if the issue persists."
                : message);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
