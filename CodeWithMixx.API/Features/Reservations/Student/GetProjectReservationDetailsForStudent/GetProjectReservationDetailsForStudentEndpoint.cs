using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Student.GetProjectReservationDetailsForStudent;

public class GetProjectReservationDetailsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/projects/{reservationId:int}", async (
                [AsParameters] GetProjectReservationDetailsForStudentRequest request,
                IHandler<GetProjectReservationDetailsForStudentRequest, Result<GetProjectReservationDetailsForStudentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<GetProjectReservationDetailsForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
