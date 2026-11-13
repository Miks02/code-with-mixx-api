using CodeWithMixx.API.Common.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Infrastructure.Exceptions.Handlers;

public class UnauthorizedAccessExceptionHandler(
    ILogger<UnauthorizedAccessExceptionHandler> logger,
    IProblemDetailsService problemDetailsService)  : IExceptionHandler
{   
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedAccessException ex)
            return false;
        
        var userIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        var traceId = httpContext.TraceIdentifier;
        
        logger.LogWarning(ex, "Unauthorized access attempt from IP: {UserIp}, Trace ID: {TraceId}", userIp, traceId);
        
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = "urn:code-with-mixx-api:error:AuthError.Unauthorized",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "User does not have access to the requested resource.",
                Instance = httpContext.Request.Path
            }
        });

    }
}