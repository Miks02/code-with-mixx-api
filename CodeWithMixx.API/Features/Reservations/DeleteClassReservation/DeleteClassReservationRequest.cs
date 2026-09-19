using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.DeleteClassReservation;

public record DeleteClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
