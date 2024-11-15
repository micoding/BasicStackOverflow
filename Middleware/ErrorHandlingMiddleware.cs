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
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
}