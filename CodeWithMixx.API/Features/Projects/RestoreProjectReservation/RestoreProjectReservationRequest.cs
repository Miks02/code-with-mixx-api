using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.RestoreProjectReservation;

public record RestoreProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
