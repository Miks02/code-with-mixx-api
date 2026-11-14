using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiries;

public class DeleteInquiriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/inquiries", async (
                    [AsParameters] DeleteInquiriesRequest request,
                    IHandler<DeleteInquiriesRequest, DeleteInquiriesResponse> handler,
                    CancellationToken ct)
                => await handler.HandleAsync(request, ct))
            .RequireAuthorization("AdminOnly")
            .WithTags("Inquiries")
            .Produces<DeleteInquiriesResponse>()
            .Produces(StatusCodes.Status400BadRequest);
    }
}
