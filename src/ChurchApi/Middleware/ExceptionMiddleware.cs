using System.Net;
using ChurchApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
        string title;
        string detail;
        string type;

        switch (exception)
        {
            case ConflictException conflict:
                statusCode = HttpStatusCode.Conflict;
                title = "Conflict";
                detail = conflict.Message;
                type = "https://httpstatuses.com/409";
                break;
            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                title = "Resource not found";
                detail = notFound.Message;
                type = "https://httpstatuses.com/404";
                break;
            case UnauthorizedException unauthorized:
                statusCode = HttpStatusCode.Unauthorized;
                title = "Unauthorized";
                detail = unauthorized.Message;
                type = "https://httpstatuses.com/401";
                break;
            case ValidationException validation:
                statusCode = HttpStatusCode.BadRequest;
                title = "Validation error";
                detail = validation.Message;
                type = "https://httpstatuses.com/400";
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                title = "Internal server error";
                detail = "An unexpected error occurred.";
                type = "https://httpstatuses.com/500";
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        return context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json");
    }
}
