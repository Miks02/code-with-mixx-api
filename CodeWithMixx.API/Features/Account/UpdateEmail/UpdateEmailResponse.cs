namespace CodeWithMixx.API.Features.Account.UpdateEmail;

public record UpdateEmailResponse
{
    public string Email { get; init; } = null!;
};