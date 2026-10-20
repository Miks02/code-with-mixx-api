using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationDetails;

public record GetProjectReservationDetailsRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
