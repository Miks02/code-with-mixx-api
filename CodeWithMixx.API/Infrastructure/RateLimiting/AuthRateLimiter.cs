using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeWithMixx.API.Infrastructure.RateLimiting;

public static class AuthRateLimiter
{
    public static void AddAuthRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("AuthLimiter", context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                });
            });
        });

    }
    
}