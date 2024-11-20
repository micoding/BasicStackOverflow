using System.Net;
using BasicStackOverflow.Exceptions;

namespace BasicStackOverflow.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

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
        catch (QuestionResolvedException qre)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(qre.Message);
        }
        catch (ForbidException fe)
        {
            context.Response.StatusCode = 403;
        }
        catch (BadRequestException bre)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(bre.Message);
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