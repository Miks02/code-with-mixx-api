using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Features.Reservations.Common;

namespace CodeWithMixx.API.Features.Reservations.UpdateClassReservation;

public record UpdateClassReservationRequest
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public decimal PaidAmount { get; init; }
    public decimal TotalPrice { get; init; }
    public string? Notes { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    
    public IReadOnlyList<int> ClassesToDelete { get; init; } = [];
    public IReadOnlyList<ClassItem> Classes { get; init; } = [];
};
