using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Inquiries.Common;

namespace CodeWithMixx.API.Features.Inquiries.GetPagedInquiries;

public class GetPagedInquiriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/inquiries", async (
                    [AsParameters] GetPagedInquiriesRequest request,
                    IHandler<GetPagedInquiriesRequest, PagedResult<InquiryItem>> handler,
                    CancellationToken ct)
                => await handler.HandleAsync(request, ct))
            .WithTags("Inquiries")
            .RequireAuthorization("AdminOnly")
            .Produces<PagedResult<InquiryItem>>();
    }
}
