using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.RestoreProjectReservation;

public record RestoreProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
