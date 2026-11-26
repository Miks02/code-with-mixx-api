using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Admin.RestoreProjectReservation;

public record RestoreProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
