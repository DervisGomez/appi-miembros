using System.Net;
using System.Text.Json;
using ChurchApi.Exceptions;

namespace ChurchApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string message;

        switch (exception)
        {
            case ConflictException conflict:
                statusCode = HttpStatusCode.Conflict;
                message = conflict.Message;
                break;
            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                message = notFound.Message;
                break;
            case UnauthorizedException unauthorized:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorized.Message;
                break;
            case ValidationException validation:
                statusCode = HttpStatusCode.BadRequest;
                message = validation.Message;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode = (int)statusCode,
            message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
