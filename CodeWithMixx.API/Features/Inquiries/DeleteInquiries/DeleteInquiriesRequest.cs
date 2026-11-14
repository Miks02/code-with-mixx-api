using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiries;

public record DeleteInquiriesRequest
{
    [FromQuery(Name = "inquiryIds")]
    public int[] InquiryIds { get; init; } = [];
};
