using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Projects.GetProjectDetails;

public class GetProjectDetailsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/reservations/projects/items/{projectId:int}", async (
                [AsParameters] GetProjectDetailsRequest request,
                IHandler<GetProjectDetailsRequest, Result<GetProjectDetailsResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Projects")
        .Produces<GetProjectDetailsResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
