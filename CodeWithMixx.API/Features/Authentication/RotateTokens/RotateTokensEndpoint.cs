using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.RotateTokens;

public class RotateTokensEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/rotate-tokens", async (IHandler<RotateTokensRequest, Result> rotateTokensHandler, CancellationToken ct = default) =>
        {
            var result = await rotateTokensHandler.HandleAsync(new RotateTokensRequest(), ct);
            return result.ToTypedResult();
        })
        .RequireRateLimiting("AuthLimiter")
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}