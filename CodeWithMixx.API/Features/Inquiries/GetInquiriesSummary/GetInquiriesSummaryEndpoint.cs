using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Inquiries.GetInquiriesSummary;

public class GetInquiriesSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/inquiries/summary", async (
                    [AsParameters] GetInquiriesSummaryRequest request,
                    IHandler<GetInquiriesSummaryRequest, GetInquiriesSummaryResponse> handler,
                    CancellationToken ct)
                => await handler.HandleAsync(request, ct))
            .WithTags("Inquiries")
            .RequireAuthorization("AdminOnly")
            .Produces<GetInquiriesSummaryResponse>();
    }
}
