using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Classes.GetPagedClassReservations;

public record GetPagedClassReservationsResponse
{
    public int Id { get; init; }
    public string StudentId { get; init; } = null!;
    public string StudentFullName { get; init; } = null!;
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime StartsAt { get; init; }
    public IReadOnlyList<ClassDto> Classes { get; init; } = [];

    public record ClassDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public string SubjectName { get; init; } = null!;
        public decimal Price { get; init; }
        public DateTime StartsAt { get; init; }
        public DateTime EndsAt { get; init; }
    }
}
