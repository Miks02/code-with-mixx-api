namespace CodeWithMixx.API.Features.Inquiries.SendInquiry;

public record SendInquiryResponse
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; init; }
};
