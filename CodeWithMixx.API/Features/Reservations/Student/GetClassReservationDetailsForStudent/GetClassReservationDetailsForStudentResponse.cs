using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;

namespace CodeWithMixx.API.Features.Reservations.Student.GetClassReservationDetailsForStudent;

public record GetClassReservationDetailsForStudentResponse
{
    public int Id { get; init; }
    public string AdminId { get; init; } = null!;
    public string AdminFullName { get; init; } = null!;
    public string AdminEmail { get; init; } = null!;
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal Bonus { get; init; }
    public string? Description { get; init; }
    public DateOnly ReservedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<ClassItem> Classes { get; init; } = [];
}
