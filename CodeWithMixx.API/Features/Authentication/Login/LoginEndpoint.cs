using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (LoginRequest request, LoginHandler loginHandler, CancellationToken ct = default) =>
        {
            var result = await loginHandler.Handle(request, ct);
            return result.ToTypedResult();
        })
        .RequireRateLimiting("AuthLimiter")
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}