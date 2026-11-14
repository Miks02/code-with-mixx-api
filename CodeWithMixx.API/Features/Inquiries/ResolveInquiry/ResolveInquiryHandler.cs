using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Inquiries;
using CodeWithMixx.API.Features.Inquiries.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Inquiries.ResolveInquiry;

public class ResolveInquiryHandler(AppDbContext context, IUserProvider userProvider) : IHandler<ResolveInquiryRequest, Result<InquiryItem>>
{
    public async Task<Result<InquiryItem>> HandleAsync(ResolveInquiryRequest request, CancellationToken ct = default)
    {
        var adminId = userProvider.GetUserId();

        var projectedInquiry = await context.Inquiries
            .Where(i => i.Id == request.InquiryId)
            .Select(i => new
            {
                Inquiry = i,
                AdminName = context.Users
                    .Where(u => u.Id == adminId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (projectedInquiry is null)
            return Result<InquiryItem>.Failure(InquiryError.NotFound(request.InquiryId));

        var inquiry = projectedInquiry.Inquiry;
        var resolveResult = inquiry.Resolve(adminId);

        if (!resolveResult.IsSuccess)
            return Result<InquiryItem>.Failure(resolveResult.Errors[0]);

        await context.SaveChangesAsync(ct);

        return Result<InquiryItem>.Success(new InquiryItem
        {
            Id = inquiry.Id,
            FirstName = inquiry.FirstName,
            LastName = inquiry.LastName,
            Email = inquiry.Email,
            PhoneNumber = inquiry.PhoneNumber,
            Message = inquiry.Message,
            InquiryType = inquiry.InquiryType,
            IsResolved = true,
            ResolvedAt = inquiry.ResolvedAt,
            ResolvedByAdminId = inquiry.ResolvedByAdmin,
            ResolvedByAdminName = projectedInquiry.AdminName,
            CreatedAt = inquiry.CreatedAt,
        });
    }
}
