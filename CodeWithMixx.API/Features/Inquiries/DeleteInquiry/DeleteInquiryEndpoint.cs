using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Inquiries.DeleteInquiry;

public class DeleteInquiryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/admin/inquiries/{inquiryId:int}", async (
                [AsParameters] DeleteInquiryRequest request,
                IHandler<DeleteInquiryRequest, Result> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult(HttpStatusCode.NoContent);
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Inquiries")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
