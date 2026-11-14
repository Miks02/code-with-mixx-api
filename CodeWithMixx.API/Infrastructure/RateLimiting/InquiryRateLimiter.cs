using System.Threading.RateLimiting;

namespace CodeWithMixx.API.Infrastructure.RateLimiting;

public static class InquiryRateLimiter
{
    public static void AddInquiryRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("InquiryLimiter", context =>
            {
                var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                return RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 3,
                    TokensPerPeriod = 1,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(10),
                    AutoReplenishment = true
                });
            });
        });
    }
}
