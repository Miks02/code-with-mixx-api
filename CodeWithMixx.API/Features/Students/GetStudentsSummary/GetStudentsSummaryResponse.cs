using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Users;

namespace CodeWithMixx.API.Features.Students.GetStudentsSummary;

public record GetStudentsSummaryResponse
{
    public int ActiveStudents { get; init; }
    public int DeletedStudents { get; init; }
    public PagedResult<StudentItem> PagedStudents { get; init; } = null!;
    public MostActiveStudentItem? MostActiveStudent { get; init; }

    public record StudentItem
    {
        public string Id { get; init; } = null!;
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string PhoneNumber { get; init; } = null!;
        public string? University { get; init; }
        public AccountStatus AccountStatus { get; init; }
        public int TotalReservations { get; init; }
        public int TotalClasses { get; init; }
        public int TotalProjects { get; init; }
        public DateTime RegisteredAt { get; init; }
        public DateTime? DeletedAt { get; init; }

    }

    public record MostActiveStudentItem
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
    }
};