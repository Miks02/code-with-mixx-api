namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

public record GetPagedSubjectsRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}
