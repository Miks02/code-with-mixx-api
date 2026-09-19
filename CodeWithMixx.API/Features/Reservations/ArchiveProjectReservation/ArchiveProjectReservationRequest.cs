using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.ArchiveProjectReservation;

public record ArchiveProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
