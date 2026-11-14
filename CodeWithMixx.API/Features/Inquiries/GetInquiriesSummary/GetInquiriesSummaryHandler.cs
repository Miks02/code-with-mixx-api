using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Inquiries.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Inquiries.GetInquiriesSummary;

public class GetInquiriesSummaryHandler(AppDbContext context) : IHandler<GetInquiriesSummaryRequest, GetInquiriesSummaryResponse>
{
    public async Task<GetInquiriesSummaryResponse> HandleAsync(GetInquiriesSummaryRequest request, CancellationToken ct = default)
    {
        var inquiriesQuery = context.Inquiries
            .ApplyFilters(request.Filters)
            .ApplySearch(request.SearchTerm)
            .ApplySort(request.SortBy)
            .ToInquiryItems();

        var pagedInquiries = await PagedResult<InquiryItem>.CreateAsync(inquiriesQuery, request.PageNumber, request.PageSize, ct);

        var stats = await context.Inquiries
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Resolved = g.Count(i => i.ResolvedAt != null),
            })
            .FirstOrDefaultAsync(ct) ?? new { Total = 0, Resolved = 0 };

        return new GetInquiriesSummaryResponse
        {
            TotalInquiries = stats.Total,
            ResolvedInquiries = stats.Resolved,
            UnresolvedInquiries = stats.Total - stats.Resolved,
            PagedInquiries = pagedInquiries,
        };
    }
}
