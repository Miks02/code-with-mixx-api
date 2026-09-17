using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.ArchiveSubject;

public class ArchiveSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/subjects/{id:int}/archive", async (
                [AsParameters] ArchiveSubjectRequest request,
                IHandler<ArchiveSubjectRequest, Result> archiveSubjectHandler,
                CancellationToken ct) =>
        {
            var result = await archiveSubjectHandler.HandleAsync(request, ct);
            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
