namespace CodeWithMixx.API.Common.Result;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    TooManyRequests
}

public record Error
{
    public string Code { get; init; }
    public string Description { get; init; }
    public ErrorType Type { get; init; }

    public Error(string code, string description, ErrorType type = ErrorType.Failure)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be empty", nameof(code));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Error description cannot be empty", nameof(description));

        Code = code;
        Description = description;
        Type = type;
    }
}