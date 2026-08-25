using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Infrastructure.Exceptions.Handlers;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var instance = httpContext.Request.Path;
        
        logger.LogWarning("An unhandled exception has occurred while executing the request. TraceID: {traceId}, Instance: {instance}, Exception: {Exception}"
            , traceId, instance, exception);


        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = "urn:codewithmixx:api:error:General.InternalServerError",
                Title = "Server error occured",
                Status = StatusCodes.Status500InternalServerError,
                Detail =
                    "An unexpected error occurred while processing the request. Please try again later or contact support if the problem persists.",
                Instance = instance
            }
        });

    }
}