namespace CodeWithMixx.API.Features.Account.RequestEmailUpdate;

public record RequestEmailUpdateRequest
{
    public string NewEmail { get; init; } = null!;
    public string Password { get; init; } = null!;
};