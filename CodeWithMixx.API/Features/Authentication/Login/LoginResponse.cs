using CodeWithMixx.API.Domain.Entities.Users;
using CodeWithMixx.API.Features.Authentication.Common;

namespace CodeWithMixx.API.Features.Authentication.Login;

public record LoginResponse
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public DateTime LastLogin { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<ActiveSessionItem> ActiveSessions { get; init; } = [];
};
