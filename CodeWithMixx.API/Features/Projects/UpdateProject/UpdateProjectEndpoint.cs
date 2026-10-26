using System.Net;
using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Projects.UpdateProject;

public class UpdateProjectEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/reservations/projects/items/{projectId:int}", async (
                int projectId,
                UpdateProjectRequest request,
                IHandler<UpdateProjectRequest, Result> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request with { ProjectId = projectId }, ct);
            return result.ToTypedResult(HttpStatusCode.NoContent);
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Projects")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
