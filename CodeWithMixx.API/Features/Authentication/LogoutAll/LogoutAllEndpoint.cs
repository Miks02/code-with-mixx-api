using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.LogoutAll;

public class LogoutAllEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout-all", async (IHandler<LogoutAllRequest, Result> handler) =>
        {
            await handler.HandleAsync(new LogoutAllRequest());
        })
        .RequireRateLimiting("AuthLimiter")
        .RequireAuthorization()
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}