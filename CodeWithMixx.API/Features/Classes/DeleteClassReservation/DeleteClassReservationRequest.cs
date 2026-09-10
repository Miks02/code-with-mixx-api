using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.DeleteClassReservation;

public record DeleteClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
