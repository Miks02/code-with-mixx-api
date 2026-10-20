namespace CodeWithMixx.API.Common.Interfaces;

public interface IUserClock
{
    TimeZoneInfo TimeZone { get; }
    DateOnly Today { get; }
    DateTimeOffset LocalNow { get; }
}