using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Infrastructure.Utility;

public class UserClock(IHttpContextAccessor http, TimeProvider timeProvider) : IUserClock
{
    private const string FallBackZoneId = "Europe/Belgrade";
    public TimeZoneInfo TimeZone => field ??= ResolveTimeZone();
    
    public DateTimeOffset LocalNow => TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), TimeZone);
    
    public DateOnly Today => DateOnly.FromDateTime(LocalNow.DateTime);

    private TimeZoneInfo ResolveTimeZone()
    {
        var clientTimeZoneId = http.HttpContext?.Request.Headers["TimeZoneId"].ToString();

        if (!string.IsNullOrWhiteSpace(clientTimeZoneId) &&
            TimeZoneInfo.TryFindSystemTimeZoneById(clientTimeZoneId, out var clientTimeZone))
        {
            return clientTimeZone;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(FallBackZoneId, out var fallbackZone))
        {
            return fallbackZone;
        }

        return TimeZoneInfo.Utc;
    }
}