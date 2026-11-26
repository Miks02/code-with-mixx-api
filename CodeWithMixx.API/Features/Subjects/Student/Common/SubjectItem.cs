namespace CodeWithMixx.API.Features.Subjects.Student.Common;

public record SubjectItem
{
    public int Id { get; init; }
    public string SubjectName { get; init; } = null!;
    public string SubjectDescription { get; init; } = null!;
    public int ClassesCount { get; init; }
}
