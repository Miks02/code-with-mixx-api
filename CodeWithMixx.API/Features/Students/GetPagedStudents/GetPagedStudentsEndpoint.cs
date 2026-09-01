using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public class GetPagedStudentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("students", async (
                [AsParameters] GetPagedStudentsRequest request,
                IHandler<GetPagedStudentsRequest, Result<PagedResult<GetPagedStudentsResponse>>> getPagedStudentsHandler,
                CancellationToken ct) =>
        {
            var result = await getPagedStudentsHandler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithTags("Students")
        .Produces<PagedResult<GetPagedStudentsResponse>>();
    }
}
