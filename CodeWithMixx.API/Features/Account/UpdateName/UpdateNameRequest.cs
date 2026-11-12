namespace CodeWithMixx.API.Features.Account.UpdateName;

public record UpdateNameRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
}
