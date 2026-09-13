using CodeWithMixx.API.Features.Subjects.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;

public record GetSubjectsSummaryForAdminRequest
{
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; }
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; init; }
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
    [FromQuery(Name = "sortBy")]
    public SubjectsSortBy SortBy { get; init; } = SubjectsSortBy.CreatedAtDescending;
};