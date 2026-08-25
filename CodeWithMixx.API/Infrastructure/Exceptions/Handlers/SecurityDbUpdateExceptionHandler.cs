using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Infrastructure.Exceptions.Handlers;

public class SecurityDbUpdateExceptionHandler(
    ILogger<SecurityDbUpdateExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not SecurityDbUpdateException ex)
            return false;

        var userId = ex.UserId;
        var userIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var traceId = httpContext.TraceIdentifier;
        
        logger.LogCritical(exception, 
            "SecurityDbUpdateException occurred - Manual intervention required. TraceId: {TraceId}, User IP: {UserIp}, User ID: {UserId}", 
            traceId, userIp, userId);
        
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "urn:code-with-mixx-api:error:General.InternalServerError",
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request. Please try again later.",
                Instance = httpContext.Request.Path
            }
        });
    }
}