namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public record GetPagedStudentsRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}
