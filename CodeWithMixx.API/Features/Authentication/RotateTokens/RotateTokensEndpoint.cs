using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.RotateTokens;

public class RotateTokensEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/rotate-tokens", async (RotateTokensHandler rotateTokensHandler, CancellationToken ct = default) =>
        {
            var result = await rotateTokensHandler.Handle();
            return result.ToTypedResult();
        })
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}