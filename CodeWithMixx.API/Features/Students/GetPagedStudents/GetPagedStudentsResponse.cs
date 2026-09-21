namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public record GetPagedStudentsResponse
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? University { get; init; }
    public int TotalReservations { get; init; }
    public int TotalClasses { get; init; }
    public int TotalProjects { get; init; }
    public DateTime RegisteredAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
