using CodeWithMixx.API.Common.Interfaces;

namespace CodeWithMixx.API.Features.Reservations.Student.GetProjectReservationsSummaryForStudent;

public class GetProjectReservationsSummaryForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/projects/summary", async (
                [AsParameters] GetProjectReservationsSummaryForStudentRequest request,
                IHandler<GetProjectReservationsSummaryForStudentRequest, GetProjectReservationsSummaryForStudentResponse> handler,
                CancellationToken ct)
            => await handler.HandleAsync(request, ct))
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<GetProjectReservationsSummaryForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
