namespace CodeWithMixx.API.Features.Authentication.LogoutBySessionId;

public record LogoutBySessionIdRequest
{
    public string SessionId { get; init; } = null!;
}