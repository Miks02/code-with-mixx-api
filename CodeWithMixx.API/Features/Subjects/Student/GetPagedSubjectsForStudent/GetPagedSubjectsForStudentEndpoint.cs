using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Student.Common;

namespace CodeWithMixx.API.Features.Subjects.Student.GetPagedSubjectsForStudent;

public class GetPagedSubjectsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/subjects",
            async (
                [AsParameters] GetPagedSubjectsForStudentRequest request,
                IHandler<GetPagedSubjectsForStudentRequest, PagedResult<SubjectItem>> handler,
                CancellationToken ct) =>
            {
                var result = await handler.HandleAsync(request, ct);
                return result;
            })
            .RequireAuthorization("StudentOnly")
            .WithTags("Subjects")
            .Produces<PagedResult<SubjectItem>>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
