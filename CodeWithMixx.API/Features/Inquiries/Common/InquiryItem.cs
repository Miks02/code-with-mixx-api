using CodeWithMixx.API.Domain.Entities.Inquiries;

namespace CodeWithMixx.API.Features.Inquiries.Common;

public record InquiryItem
{
    public int Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string Message { get; init; } = null!;
    public InquiryType InquiryType { get; init; }
    public bool IsResolved { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? ResolvedByAdminId { get; init; }
    public string? ResolvedByAdminName { get; init; }
    public DateTime CreatedAt { get; init; }
}
