using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

public class GetPagedSubjectsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("subjects", async (
                [AsParameters] GetPagedSubjectsRequest request,
                IHandler<GetPagedSubjectsRequest, Result<PagedResult<GetPagedSubjectsResponse>>> getPagedSubjectsHandler,
                CancellationToken ct) =>
        {
            var result = await getPagedSubjectsHandler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .WithTags("Subjects")
        .Produces<PagedResult<GetPagedSubjectsResponse>>();
    }
}
