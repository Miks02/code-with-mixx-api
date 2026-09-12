using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.GetMe;

public class GetMeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/me", async (IHandler<GetMeRequest, Result<GetMeResponse>> handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetMeRequest(), ct);
            return result.ToTypedResult();
        })
        .WithTags("Authentication")
        .RequireAuthorization()
        .Produces<Result<GetMeResponse>>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}