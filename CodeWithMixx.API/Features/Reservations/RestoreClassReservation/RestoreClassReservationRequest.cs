using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.RestoreClassReservation;

public record RestoreClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
