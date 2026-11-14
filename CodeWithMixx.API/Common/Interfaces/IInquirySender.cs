using CodeWithMixx.API.Domain.Entities.Inquiries;

namespace CodeWithMixx.API.Common.Interfaces;

public interface IInquirySender
{
    Task SendInquiryAsync(Inquiry inquiry);
}