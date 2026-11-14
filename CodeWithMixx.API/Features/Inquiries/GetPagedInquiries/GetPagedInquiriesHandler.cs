using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Inquiries.Common;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.Inquiries.GetPagedInquiries;

public class GetPagedInquiriesHandler(AppDbContext context) : IHandler<GetPagedInquiriesRequest, PagedResult<InquiryItem>>
{
    public async Task<PagedResult<InquiryItem>> HandleAsync(GetPagedInquiriesRequest request, CancellationToken ct = default)
    {
        var inquiriesQuery = context.Inquiries
            .ApplyFilters(request.Filters)
            .ApplySearch(request.SearchTerm)
            .ApplySort(request.SortBy)
            .ToInquiryItems();

        return await PagedResult<InquiryItem>.CreateAsync(inquiriesQuery, request.PageNumber, request.PageSize, ct);
    }
}
