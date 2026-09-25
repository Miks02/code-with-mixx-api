using System.Threading.RateLimiting;

namespace CodeWithMixx.API.Infrastructure.RateLimiting;

public static class InvitationRateLimiter
{
    public static void AddInvitationRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("InvitationLimiter", context =>
                RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new TokenBucketRateLimiterOptions
                    {
                        TokensPerPeriod = 5,
                        TokenLimit = 10,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        AutoReplenishment = true
                    }));
        });
    }
}