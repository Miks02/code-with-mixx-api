using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.Student.GetPagedProjectReservationsForStudent;

public class GetPagedProjectReservationsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/projects", async (
                [AsParameters] GetPagedProjectReservationsForStudentRequest request,
                IHandler<GetPagedProjectReservationsForStudentRequest, PagedResult<GetPagedProjectReservationsForStudentResponse>> handler,
                CancellationToken ct)
            => await handler.HandleAsync(request, ct))
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<PagedResult<GetPagedProjectReservationsForStudentResponse>>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
