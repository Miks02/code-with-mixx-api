namespace CodeWithMixx.API.Features.Subjects.GetSubjectById;

public record GetSubjectByIdResponse
{
    public int Id { get; init; }
    public string SubjectName { get; init; } = null!;
    public string Description { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}