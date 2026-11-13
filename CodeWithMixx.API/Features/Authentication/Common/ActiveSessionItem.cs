namespace CodeWithMixx.API.Features.Authentication.Common;

public record ActiveSessionItem
{
    public Guid SessionId { get; init; }
    public string Os { get; init; } = null!;
    public string Device { get; init; } = null!;
    public string Browser { get; init; } = null!;
    public DateTime ActiveSince { get; init; }
    public bool IsCurrentSession { get; init; }
}
