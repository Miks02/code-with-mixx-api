using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.ArchiveProjectReservation;

public record ArchiveProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
