namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

public record GetPagedSubjectsResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}
