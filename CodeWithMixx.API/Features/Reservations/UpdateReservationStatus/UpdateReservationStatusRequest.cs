using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.UpdateReservationStatus;

public record UpdateReservationStatusRequest
{
    public int ReservationId { get; init; }
    public UpdateReservationStatusBody Body { get; init; } = null!;
};

public record UpdateReservationStatusBody
{
    public ReservationStatus ReservationStatus { get; init; }
}
