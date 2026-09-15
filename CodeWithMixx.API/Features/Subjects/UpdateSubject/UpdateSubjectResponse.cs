namespace CodeWithMixx.API.Features.Subjects.UpdateSubject;

public record UpdateSubjectResponse
{
    public int Id { get; init; }
    public string SubjectName { get; init; } = null!;
    public string SubjectDescription { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public int ClassesCount { get; init; }
    public int StudentsCount { get; init; }
};
