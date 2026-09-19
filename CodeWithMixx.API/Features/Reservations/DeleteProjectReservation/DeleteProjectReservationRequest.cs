using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.DeleteProjectReservation;

public record DeleteProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
