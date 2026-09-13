using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;

public class GetSubjectsSummaryForAdminEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/subjects/summary",
            async (
                [AsParameters] GetSubjectsSummaryForAdminRequest request,
                IHandler<GetSubjectsSummaryForAdminRequest, GetSubjectsSummaryForAdminResponse> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("AdminOnly")
            .WithTags("Subjects")
            .Produces<GetSubjectsSummaryForAdminResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}