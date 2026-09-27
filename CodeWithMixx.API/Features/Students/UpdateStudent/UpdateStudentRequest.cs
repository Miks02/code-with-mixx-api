using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.UpdateStudent;

public record UpdateStudentRequest
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? University { get; init; }
}