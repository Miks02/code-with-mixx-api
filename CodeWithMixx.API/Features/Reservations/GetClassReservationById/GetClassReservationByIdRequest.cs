using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationById;

public record GetClassReservationByIdRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};