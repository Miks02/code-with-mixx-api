using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.CreateSubject;

public class CreateSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/subjects", async (CreateSubjectRequest request, IHandler<CreateSubjectRequest, Result<CreateSubjectResponse>> createSubjectHandler) =>
        {
            var result = await createSubjectHandler.HandleAsync(request);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}