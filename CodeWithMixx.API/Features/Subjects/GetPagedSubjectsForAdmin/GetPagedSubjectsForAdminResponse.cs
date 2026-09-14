using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public class GetPagedSubjectsForAdminResponse
{
    public PagedResult<SubjectItem> PagedSubjects { get; init; } = null!;

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
