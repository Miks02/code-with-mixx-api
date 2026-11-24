namespace CodeWithMixx.API.Infrastructure.Exceptions;

public class RefreshTokenReuseException : Exception
{
    public string UserId { get; }
    public Guid SessionId { get; }

    public RefreshTokenReuseException(string userId, Guid sessionId)
        : base($"Reuse of an already rotated refresh token detected for session '{sessionId}'. All user's sessions have been revoked.")
    {
        UserId = userId;
        SessionId = sessionId;
    }
}
