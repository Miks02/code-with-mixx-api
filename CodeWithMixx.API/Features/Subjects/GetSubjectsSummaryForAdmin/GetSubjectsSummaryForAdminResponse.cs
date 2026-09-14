using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;

public class GetSubjectsSummaryForAdminResponse
{
    public PagedResult<SubjectItem> PagedSubjects { get; init; } = null!;
    public int TotalSubjects { get; init; }
    public int TaughtSubjects { get; init; }
    public int UntaughtSubjects { get; init; } 
    public SubjectItem? MostPopularSubject { get; init; }

    public record SubjectItem
    {
        public int Id { get; init; }
        public string SubjectName { get; init; } = null!;
        public string SubjectDescription { get; init; } = null!;
        public int ClassesCount { get; init; }
        public int StudentsCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}