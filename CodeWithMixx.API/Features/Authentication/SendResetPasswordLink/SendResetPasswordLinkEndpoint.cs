using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Authentication.SendResetPasswordLink;

public class SendResetPasswordLinkEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/forgot-password", async (
                SendResetPasswordLinkRequest request,
                IHandler<SendResetPasswordLinkRequest, Result> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result.ToTypedResult();
            })
            .WithTags("Authentication")
            .RequireRateLimiting("ForgotPasswordLimiter")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}