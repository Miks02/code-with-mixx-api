namespace CodeWithMixx.API.Features.Authentication.ResetPassword;

public record ResetPasswordRequest
{
    public string UserId { get; init; } = null!;
    public string Token { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string ConfirmedPassword { get; init; } = null!;
};
