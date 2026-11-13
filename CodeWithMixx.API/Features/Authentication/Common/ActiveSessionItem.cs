namespace CodeWithMixx.API.Features.Authentication.Common;

public record ActiveSessionItem
{
    public Guid SessionId { get; init; }
    public string Os { get; init; } = null!;
    public SessionDevice Device { get; init; }
    public string Browser { get; init; } = null!;
    public DateTime ActiveSince { get; init; }
    public bool IsCurrentSession { get; init; }
}
