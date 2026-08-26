using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (IHandler<LogoutRequest, Result> logoutHandler, CancellationToken ct = default) =>
            {
                var result = await logoutHandler.HandleAsync(new LogoutRequest(), ct);
                return result.ToTypedResult();
            })
            .RequireRateLimiting("AuthLimiter")
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK);
    }
}