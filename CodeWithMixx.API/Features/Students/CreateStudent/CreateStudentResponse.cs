namespace CodeWithMixx.API.Features.Students.CreateStudent
{
    public record CreateStudentResponse
    {
        public string Id { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
        public string? University { get; init; }
    }
}
