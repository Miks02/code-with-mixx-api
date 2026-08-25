using Microsoft.AspNetCore.Http.HttpResults;

namespace CodeWithMixx.API.Infrastructure.Filters;

public class ProblemDetailsFilter(ILogger<ProblemDetailsFilter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        
        if(result is not ProblemHttpResult problemHttpResult)
            return result;
        
        problemHttpResult.ProblemDetails.Instance = context.HttpContext.Request.Path;

        var problemDetails = problemHttpResult.ProblemDetails;

        logger.LogInformation("Request failed: {status} {title} at {path} | ErrorCode: {code}",
            problemDetails.Status,
            problemDetails.Title,
            context.HttpContext.Request.Path,       
            problemDetails.Extensions["errorCode"]);

        return result;
    }
}