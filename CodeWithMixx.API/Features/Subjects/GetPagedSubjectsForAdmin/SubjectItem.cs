namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public record SubjectItem
{
    public int Id { get; init; }
    public string SubjectName { get; init; } = null!;
    public string SubjectDescription { get; init; } = null!;
    public int ClassesCount { get; init; }
    public int StudentsCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
