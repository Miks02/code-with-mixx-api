using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Inquiries.ResolveInquiry;

public record ResolveInquiryRequest
{
    [FromRoute(Name = "inquiryId")]
    public int InquiryId { get; init; }
};
