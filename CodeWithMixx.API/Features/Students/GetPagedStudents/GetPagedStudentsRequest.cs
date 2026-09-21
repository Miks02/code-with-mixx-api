using CodeWithMixx.API.Features.Students.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public record GetPagedStudentsRequest
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
    [FromQuery(Name = "sortBy")]
    public StudentsSortBy? SortBy { get; init; }
    [FromQuery(Name = "filter")]
    public StudentsFilterBy? Filter { get; init; }
    [FromQuery(Name = "includeDeleted")]
    public bool IncludeDeleted { get; init; } = false;
}
