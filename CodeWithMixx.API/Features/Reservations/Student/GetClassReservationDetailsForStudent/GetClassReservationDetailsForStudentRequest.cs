using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationDetailsForStudent;

public record GetClassReservationDetailsForStudentRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
