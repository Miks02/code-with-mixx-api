namespace CodeWithMixx.API.Features.Account.UpdateEmail;

public record UpdateEmailRequest
{
    public string Token { get; init; } = null!;
    public string NewEmail { get; init; } = null!;
    public string Password { get; init; } = null!;
};