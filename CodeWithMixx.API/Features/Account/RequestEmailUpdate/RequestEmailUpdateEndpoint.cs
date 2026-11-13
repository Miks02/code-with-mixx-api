using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.RequestEmailUpdate;

public class RequestEmailUpdateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("account/email-change-request", async (
                RequestEmailUpdateRequest request,
                IHandler<RequestEmailUpdateRequest, Result> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("AuthLimiter")
            .WithTags("Account")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}