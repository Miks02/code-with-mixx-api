using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Infrastructure.Utility;

public static class UtilityRegistration
{
    public static void AddUtility(this IServiceCollection services)
    {
        services.AddScoped<IUserClock, UserClock>();
    }
}