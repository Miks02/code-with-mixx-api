using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.UpdateSubject;

public class UpdateSubjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/subjects/{id:int}", async (
                int id,
                UpdateSubjectRequest request,
                IHandler<UpdateSubjectRequest, Result<UpdateSubjectResponse>> updateSubjectHandler,
                CancellationToken ct) =>
        {
            var result = await updateSubjectHandler.HandleAsync(request with { Id = id }, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Subjects")
        .ProducesValidationProblem()
        .Produces<UpdateSubjectResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
