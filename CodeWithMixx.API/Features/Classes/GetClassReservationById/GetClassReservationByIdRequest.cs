using Microsoft.AspNetCore.Mvc;

namespace CodeWithMixx.API.Features.Classes.GetClassReservationById;

public record GetClassReservationByIdRequest
{
    [FromRoute]
    public int ReservationId { get; init; }
};