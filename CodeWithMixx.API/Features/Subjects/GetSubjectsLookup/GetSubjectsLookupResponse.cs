namespace CodeWithMixx.API.Features.Subjects.GetSubjectsLookup;

public record GetSubjectsLookupResponse
{
    public int Id { get; init; }
    public string SubjectName { get; init; } = null!;
};