namespace CodeWithMixx.API.Features.Authentication.ActivateAccount;

public record ActivateAccountRequest
{
    public string UserId { get; init; } = null!;
    public string Token { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string ConfirmedPassword { get; init; } = null!;
};