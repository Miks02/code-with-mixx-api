using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.RestoreClassReservation;

public record RestoreClassReservationRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};
