using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.DeleteSubject;

public class DeleteSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("subjects/{id:int}", async (
                [AsParameters] DeleteSubjectRequest request,
                IHandler<DeleteSubjectRequest, Result> deleteSubjectHandler,
                CancellationToken ct) =>
        {
            var result = await deleteSubjectHandler.HandleAsync(request, ct);
            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
