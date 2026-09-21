using CodeWithMixx.API.Features.Students.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetStudentsSummary;

public record GetStudentsSummaryRequest
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
    [FromQuery(Name = "sortBy")]
    public StudentsSortBy? SortBy { get; init; }
    [FromQuery(Name = "filters")]
    public StudentsFilterBy[] Filters { get; init; } = [];
    [FromQuery(Name = "includeDeleted")]
    public bool IncludeDeleted { get; init; } = false;


};