namespace CodeWithMixx.API.Infrastructure.Exceptions;

public class SecurityDbUpdateException : Exception
{
    public string UserId { get; }

    public SecurityDbUpdateException(string userId, string message, Exception innerException) : base(message, innerException)
    {
        UserId = userId;
    }
}