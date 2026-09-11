using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public record UpdateClassReservationResponse
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public string? Notes { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
