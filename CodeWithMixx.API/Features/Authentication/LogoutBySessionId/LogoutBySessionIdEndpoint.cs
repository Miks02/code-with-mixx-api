using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.LogoutBySessionId;

public class LogoutBySessionIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout-session", async (LogoutBySessionIdRequest request, IHandler<LogoutBySessionIdRequest, Result> handler) =>
        {
            var result = await handler.HandleAsync(request);
            return result.ToTypedResult();
        })
        .RequireAuthorization()
        .WithTags("Authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}