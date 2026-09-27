using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Features.Students.UpdateStudent;

public record UpdateStudentResponse
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? University { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public DateTime RegisteredAt { get; init; }
};