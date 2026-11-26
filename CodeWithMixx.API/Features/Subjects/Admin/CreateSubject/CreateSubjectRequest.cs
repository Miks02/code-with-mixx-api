namespace CodeWithMixx.API.Features.Subjects.Admin.CreateSubject;

public record CreateSubjectRequest
{
    public string SubjectName { get; init; } = null!;
    public string SubjectDescription { get; init; } = null!;
};