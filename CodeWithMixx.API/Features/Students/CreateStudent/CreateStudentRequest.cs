namespace CodeWithMixx.API.Features.Students.CreateStudent
{
    public record CreateStudentRequest
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
        public string? University { get; init; }
    }
}
