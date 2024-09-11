using System.Net;

namespace rag_2_backend.Config;

public record ExceptionResponse(HttpStatusCode StatusCode, string Description);

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An unexpected error occurred.");

        var response = exception switch
        {
            BadHttpRequestException e => new ExceptionResponse(HttpStatusCode.BadRequest, e.Message),
            KeyNotFoundException e => new ExceptionResponse(HttpStatusCode.NotFound, e.Message),
            UnauthorizedAccessException e => new ExceptionResponse(HttpStatusCode.Unauthorized, e.Message),
            _ => new ExceptionResponse(HttpStatusCode.InternalServerError, "Internal server error. Please retry later.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)response.StatusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}