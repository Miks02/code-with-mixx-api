using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.ChangePassword;

public class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("account/password", async (
                ChangePasswordRequest request,
                IHandler<ChangePasswordRequest, Result> changePasswordHandler) =>
        {
            var result = await changePasswordHandler.HandleAsync(request);
            return result.ToTypedResult();
        })
        .RequireRateLimiting("AuthLimiter")
        .RequireAuthorization()
        .WithTags("Account")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}