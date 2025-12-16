using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace WebApplication1;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var pgEx = (exception as DbUpdateException)?.InnerException as PostgresException;

        var response = new 
        {
            error = pgEx?.MessageText ?? exception.Message,
            trace = pgEx is null ? exception.StackTrace : null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return context.Response.WriteAsJsonAsync(response);
    }
}