using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Authentication.ResetPassword;

public class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/reset-password", async (
                [FromBody] ResetPasswordRequest request,
                IHandler<ResetPasswordRequest, Result> resetPasswordHandler, CancellationToken ct) =>
            {
                var result = await resetPasswordHandler.HandleAsync(request, ct);
                return result.ToTypedResult();
            })
            .WithTags("Authentication")
            .RequireRateLimiting("AuthLimiter")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
