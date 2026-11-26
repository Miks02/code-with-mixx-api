using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Student.GetProjectReservationDetailsForStudent;

public record GetProjectReservationDetailsForStudentRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
