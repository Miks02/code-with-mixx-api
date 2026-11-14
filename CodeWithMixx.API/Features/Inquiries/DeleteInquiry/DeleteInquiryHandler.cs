using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Inquiries;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiry;

public class DeleteInquiryHandler(AppDbContext context) : IHandler<DeleteInquiryRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteInquiryRequest request, CancellationToken ct = default)
    {
        var deletedRows = await context.Inquiries
            .Where(i => i.Id == request.InquiryId)
            .ExecuteDeleteAsync(ct);

        return deletedRows == 0
            ? Result.Failure(InquiryError.NotFound(request.InquiryId))
            : Result.Success();
    }
}
