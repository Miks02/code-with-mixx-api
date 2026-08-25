namespace CodeWithMixx.API.Infrastructure.Exceptions;

public class TokensRevokedException : Exception
{
    public string UserId { get; }
    
    public TokensRevokedException(string userId, string message, Exception innerException) : base(message, innerException)
    {
        UserId = userId;
    }
}