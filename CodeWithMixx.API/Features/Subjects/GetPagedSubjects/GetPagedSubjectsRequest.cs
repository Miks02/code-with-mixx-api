using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

public record GetPagedSubjectsRequest
{
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 10;
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}
