using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Account.UpdateEmail;

public class UpdateEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("account/email", async (
            UpdateEmailRequest request, 
            IHandler<UpdateEmailRequest, Result<UpdateEmailResponse>> handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization()
        .RequireRateLimiting("AuthLimiter")
        .WithTags("Account")
        .Produces<UpdateEmailResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict);
    }
}