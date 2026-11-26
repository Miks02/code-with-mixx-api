using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationDetailsForStudent;

public class GetClassReservationDetailsForStudentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/student/reservations/classes/{reservationId:int}", async (
                [AsParameters] GetClassReservationDetailsForStudentRequest request,
                IHandler<GetClassReservationDetailsForStudentRequest, Result<GetClassReservationDetailsForStudentResponse>> handler,
                CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return result.ToTypedResult();
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Reservations")
        .Produces<GetClassReservationDetailsForStudentResponse>()
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}
