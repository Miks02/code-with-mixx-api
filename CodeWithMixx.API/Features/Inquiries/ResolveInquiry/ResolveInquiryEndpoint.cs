using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Inquiries.Common;

namespace CodeWithMixx.API.Features.Inquiries.ResolveInquiry;

public class ResolveInquiryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/inquiries/{inquiryId:int}/resolve", async (
                [AsParameters] ResolveInquiryRequest request,
                IHandler<ResolveInquiryRequest, Result<InquiryItem>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Inquiries")
            .Produces<InquiryItem>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
