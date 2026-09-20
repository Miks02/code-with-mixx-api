namespace CodeWithMixx.API.Features.Authentication.SendResetPasswordLink;

public record SendResetPasswordLinkRequest
{
    public string Email { get; init; } = null!;
};