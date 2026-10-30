using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.GetClassReservationDetails;

public record GetClassReservationDetailsRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
