using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public record UpdateClassReservationRequest
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public decimal PaidAmount { get; init; }
    public decimal TotalPrice { get; init; }
    public string? Notes { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
};
