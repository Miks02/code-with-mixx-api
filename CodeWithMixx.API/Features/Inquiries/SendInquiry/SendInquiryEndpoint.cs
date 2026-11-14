using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Inquiries.SendInquiry;

public class SendInquiryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/inquiries", async (
                SendInquiryRequest request,
                IHandler<SendInquiryRequest, Result<SendInquiryResponse>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult(HttpStatusCode.Created);
            })
            .WithTags("Inquiries")
            .RequireRateLimiting("InquiryLimiter")
            .Produces<SendInquiryResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status429TooManyRequests);
    }
}
