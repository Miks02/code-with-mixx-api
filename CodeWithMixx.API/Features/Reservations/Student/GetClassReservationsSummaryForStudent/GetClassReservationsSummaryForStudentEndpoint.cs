using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationsSummaryForStudent;

public class GetClassReservationsSummaryForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/classes/summary", async (
                [AsParameters] GetClassReservationsSummaryForStudentRequest request,
                IHandler<GetClassReservationsSummaryForStudentRequest, Result<GetClassReservationsSummaryForStudentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<GetClassReservationsSummaryForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
