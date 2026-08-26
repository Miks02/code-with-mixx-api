namespace CodeWithMixx.API.Features.Subjects.CreateSubject;

public record CreateSubjectRequest
{
    public string SubjectName { get; init; } = null!;
    public string Description { get; init; } = null!;
};