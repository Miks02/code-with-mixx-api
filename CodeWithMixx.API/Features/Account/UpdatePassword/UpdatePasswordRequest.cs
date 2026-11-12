namespace CodeWithMixx.API.Features.Account.UpdatePassword;

public record UpdatePasswordRequest
{
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
};