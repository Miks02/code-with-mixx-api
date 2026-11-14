using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiries;

public class DeleteInquiriesHandler(AppDbContext context) : IHandler<DeleteInquiriesRequest, DeleteInquiriesResponse>
{
    public async Task<DeleteInquiriesResponse> HandleAsync(DeleteInquiriesRequest request, CancellationToken ct = default)
    {
        var inquiryIds = request.InquiryIds.Distinct().ToList();

        var deletedCount = await context.Inquiries
            .Where(i =>  inquiryIds.Contains(i.Id))
            .ExecuteDeleteAsync(ct);

        return new DeleteInquiriesResponse { DeletedCount = deletedCount };
    }
}
