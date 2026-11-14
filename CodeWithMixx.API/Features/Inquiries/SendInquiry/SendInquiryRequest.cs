using CodeWithMixx.API.Domain.Entities.Inquiries;

namespace CodeWithMixx.API.Features.Inquiries.SendInquiry;

public record SendInquiryRequest
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string Message { get; init; } = null!;
    public InquiryType InquiryType { get; init; }
};
