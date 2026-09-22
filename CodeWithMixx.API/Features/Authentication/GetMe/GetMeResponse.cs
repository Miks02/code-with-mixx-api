using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Features.Authentication.GetMe;

public record GetMeResponse
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public AccountStatus AccountStatus { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
};