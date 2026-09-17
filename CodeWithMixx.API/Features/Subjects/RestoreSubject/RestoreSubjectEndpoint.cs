using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.RestoreSubject;

public class RestoreSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/subjects/{id:int}/restore", async (
                [AsParameters] RestoreSubjectRequest request,
                IHandler<RestoreSubjectRequest, Result> restoreSubjectHandler,
                CancellationToken ct) =>
        {
            var result = await restoreSubjectHandler.HandleAsync(request, ct);
            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
