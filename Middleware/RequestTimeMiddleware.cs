using System.Diagnostics;

namespace BasicStackOverflow.Middleware;

public class RequestTimeMiddleware : IMiddleware
{
    ILogger<RequestTimeMiddleware> _logger;
    Stopwatch _stopwatch;
    
    public RequestTimeMiddleware(ILogger<RequestTimeMiddleware> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _stopwatch.Start();
        await next.Invoke(context);
        _stopwatch.Stop();
        
        if(_stopwatch.ElapsedMilliseconds > 2000)
            _logger.LogInformation($"Request {context.Request.Method} at {context.Request.Path} took : {_stopwatch.ElapsedMilliseconds}ms.");
    }
}