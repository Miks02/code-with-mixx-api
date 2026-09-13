using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Classes.Common;

namespace CodeWithMixx.API.Features.Classes.GetClassReservationById;

public record GetClassReservationByIdResponse
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public string StudentFullName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public string? StudentUniversity { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<ClassItem> Classes { get; init; } = [];
}
