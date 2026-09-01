namespace CodeWithMixx.API.Features.Students.SearchStudents
{
    public record SearchStudentsResponse
    {
        public IReadOnlyList<StudentDto> Students { get; init; } = [];

        public record StudentDto
        {
            public string Id { get; init; } = null!;
            public string FullName { get; init; } = null!;
            public string Email { get; init; } = null!;
            public string? University { get; init; }
        }
    }
}
