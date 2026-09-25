using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.SendInvitation;

public class SendInvitationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/students/{id:guid}/invite", async (
                [AsParameters] SendInvitationRequest request,
                IHandler<SendInvitationRequest, Result> sendInvitationHandler,
                CancellationToken ct) =>
        {
            var result = await sendInvitationHandler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .WithTags("Students")
        .RequireAuthorization("AdminOnly")
        .RequireRateLimiting("InvitationLimiter")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}