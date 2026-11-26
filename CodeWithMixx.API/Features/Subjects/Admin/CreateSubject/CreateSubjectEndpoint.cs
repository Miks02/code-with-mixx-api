using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.Admin.CreateSubject;

public class CreateSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/subjects", async (CreateSubjectRequest request, IHandler<CreateSubjectRequest, Result<CreateSubjectResponse>> createSubjectHandler) =>
        {
            var result = await createSubjectHandler.HandleAsync(request);
            return result.ToTypedResult(HttpStatusCode.Created);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}