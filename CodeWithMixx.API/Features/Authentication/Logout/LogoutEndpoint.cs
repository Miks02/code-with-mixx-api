using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (LogoutHandler logoutHandler, CancellationToken ct = default) =>
            {
                var result = await logoutHandler.Handle(ct);
                return result.ToTypedResult();
            })
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK);
    }
}