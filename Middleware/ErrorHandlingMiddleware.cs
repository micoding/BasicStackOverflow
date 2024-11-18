using BasicStackOverflow.Exceptions;
using ILogger = Castle.Core.Logging.ILogger;

namespace BasicStackOverflow.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (NotFoundException nfe)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(nfe.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error occured.");
            throw;
        }
    }
}