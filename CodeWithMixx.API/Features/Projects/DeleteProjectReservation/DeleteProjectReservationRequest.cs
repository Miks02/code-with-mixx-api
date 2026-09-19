using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Projects.DeleteProjectReservation;

public record DeleteProjectReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
