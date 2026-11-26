using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Subjects.Student.GetSubjectsSummaryForStudent;

public class GetSubjectsSummaryForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/subjects/summary",
            async (
                [AsParameters] GetSubjectsSummaryForStudentRequest request,
                IHandler<GetSubjectsSummaryForStudentRequest, GetSubjectsSummaryForStudentResponse> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("StudentOnly")
            .WithTags("Subjects")
            .Produces<GetSubjectsSummaryForStudentResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
