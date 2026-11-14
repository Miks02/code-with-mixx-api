using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Inquiries.Common;

namespace CodeWithMixx.API.Features.Inquiries.GetInquiriesSummary;

public record GetInquiriesSummaryResponse
{
    public int TotalInquiries { get; init; }
    public int UnresolvedInquiries { get; init; }
    public int ResolvedInquiries { get; init; }
    public PagedResult<InquiryItem> PagedInquiries { get; init; } = null!;
}
