using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public class GetPagedSubjectsForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/subjects",
            async (
                [AsParameters] GetPagedSubjectsForAdminRequest request,
                IHandler<GetPagedSubjectsForAdminRequest, PagedResult<SubjectItem>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Subjects")
            .Produces<PagedResult<SubjectItem>>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
