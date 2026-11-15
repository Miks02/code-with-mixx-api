using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Features.Students.Common;

public record StudentItem
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? University { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public DateTime RegisteredAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
