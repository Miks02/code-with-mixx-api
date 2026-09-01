namespace CodeWithMixx.API.Features.Students.GetStudentById;

public record GetStudentByIdResponse
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? University { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLogin { get; init; }
}
