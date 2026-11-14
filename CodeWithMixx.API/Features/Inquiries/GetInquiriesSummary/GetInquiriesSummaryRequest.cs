using CodeWithMixx.API.Features.Inquiries.Common;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Inquiries.GetInquiriesSummary;

public record GetInquiriesSummaryRequest
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
    [FromQuery(Name = "sortBy")]
    public InquiriesSortBy? SortBy { get; init; }
    [FromQuery(Name = "filters")]
    public InquiriesFilterBy[] Filters { get; init; } = [];
}
