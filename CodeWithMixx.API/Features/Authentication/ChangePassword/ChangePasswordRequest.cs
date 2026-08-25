namespace CodeWithMixx.API.Features.Authentication.ChangePassword;

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
};