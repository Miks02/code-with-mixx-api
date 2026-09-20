using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeWithMixx.API.Infrastructure.RateLimiting;

public static class ForgotPasswordRateLimiter
{
    public static void AddForgotPasswordRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("ForgotPasswordLimiter", context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 1,
                    TokensPerPeriod = 1,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true
                });
            });
        });

    }
    
}