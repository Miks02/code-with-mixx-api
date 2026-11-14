using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Inquiries;
using CodeWithMixx.API.Infrastructure.Persistence;

namespace CodeWithMixx.API.Features.Inquiries.SendInquiry;

public class SendInquiryHandler(AppDbContext context) : IHandler<SendInquiryRequest, Result<SendInquiryResponse>>
{
    public async Task<Result<SendInquiryResponse>> HandleAsync(SendInquiryRequest request, CancellationToken ct = default)
    {
        var inquiryResult = Inquiry.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Message,
            request.InquiryType);

        if (!inquiryResult.IsSuccess)
            return Result<SendInquiryResponse>.Failure(inquiryResult.Errors[0]);

        var inquiry = inquiryResult.Payload!;

        context.Inquiries.Add(inquiry);
        await context.SaveChangesAsync(ct);

        return Result<SendInquiryResponse>.Success(new SendInquiryResponse
        {
            Id = inquiry.Id,
            CreatedAt = inquiry.CreatedAt
        });
    }
}
