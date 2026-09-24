using System.Net;
using System.Text.Json;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Exceptions;

namespace GarmentsERP.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException valEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(valEx.Errors)
            ),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(notFoundEx.Message)
            ),
            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(forbiddenEx.Message)
            ),
            AppException appEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(appEx.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.Fail("Unauthorized access.")
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred. Please contact technical support.")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
