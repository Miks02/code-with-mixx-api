using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiry;

public record DeleteInquiryRequest
{
    [FromRoute(Name = "inquiryId")]
    public int InquiryId { get; init; }
};
