using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;

namespace CodeWithMixx.API.Features.Reservations.GetPagedClassReservations;

public record GetPagedClassReservationsResponse
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public string StudentFullName { get; init; } = null!;
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime StartsAt { get; init; }
    public IReadOnlyList<ClassItem> Classes { get; init; } = [];

}
