using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Student.Common;

namespace CodeWithMixx.API.Features.Subjects.Student.GetSubjectsSummaryForStudent;

public class GetSubjectsSummaryForStudentResponse
{
    public PagedResult<SubjectItem> PagedSubjects { get; init; } = null!;
    public int TotalSubjects { get; init; }
    public int TaughtSubjects { get; init; }
    public int UntaughtSubjects { get; init; }
    public SubjectItem? MostPopularSubject { get; init; }
    public LastSubjectClassItem? LastSubjectClass { get; init; }

    public record LastSubjectClassItem
    {
        public string SubjectName { get; init; } = null!;
        public DateOnly ClassDate { get; init; }
    }
}
