using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.ArchiveClassReservation;

public record ArchiveClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
