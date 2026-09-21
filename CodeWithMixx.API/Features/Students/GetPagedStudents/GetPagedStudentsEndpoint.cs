using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public class GetPagedStudentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/students", async (
                    [AsParameters] GetPagedStudentsRequest request,
                    IHandler<GetPagedStudentsRequest, PagedResult<GetPagedStudentsResponse>> handler,
                    CancellationToken ct) 
            => await handler.HandleAsync(request, ct))
            .WithTags("Students")
            .RequireAuthorization("AdminOnly")
            .Produces<PagedResult<GetPagedStudentsResponse>>();
    }
}
