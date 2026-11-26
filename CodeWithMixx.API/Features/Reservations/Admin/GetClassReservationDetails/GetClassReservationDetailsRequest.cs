using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Reservations.Admin.GetClassReservationDetails;

public record GetClassReservationDetailsRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
