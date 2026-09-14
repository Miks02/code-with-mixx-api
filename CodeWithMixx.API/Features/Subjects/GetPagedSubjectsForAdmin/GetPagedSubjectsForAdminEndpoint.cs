using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public class GetPagedSubjectsForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/subjects",
            async (
                [AsParameters] GetPagedSubjectsForAdminRequest request,
                IHandler<GetPagedSubjectsForAdminRequest, GetPagedSubjectsForAdminResponse> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Subjects")
            .Produces<GetPagedSubjectsForAdminResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
