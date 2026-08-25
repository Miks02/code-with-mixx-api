using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CodeWithMixx.API.Infrastructure.RateLimiting;

public static class GlobalRateLimiter
{
    public static void AddGlobalRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                var problem = new ProblemDetails
                {
                    Title = "Too many requests",
                    Type = "urn:code-with-mixx-api:error:General.TooManyRequests",
                    Detail = "You have exceeded the allowed number of requests. Please try again later.",
                    Status = StatusCodes.Status429TooManyRequests,
                    Instance = context.HttpContext.Request.Path,
                    Extensions = { ["errorCode"] = "General.TooManyRequests" }
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            };
            
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                    TokensPerPeriod = 10
                });

            });
        });
    }
}