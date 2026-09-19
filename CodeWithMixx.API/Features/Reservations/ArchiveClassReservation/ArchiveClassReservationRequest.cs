using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.ArchiveClassReservation;

public record ArchiveClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
