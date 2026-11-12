using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.UpdatePassword;

public class UpdatePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("account/password", async (
                UpdatePasswordRequest request,
                IHandler<UpdatePasswordRequest, Result> updatePasswordHandler) =>
        {
            var result = await updatePasswordHandler.HandleAsync(request);
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