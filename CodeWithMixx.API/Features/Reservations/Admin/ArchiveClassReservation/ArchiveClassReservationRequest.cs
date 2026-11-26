using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Admin.ArchiveClassReservation;

public record ArchiveClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
