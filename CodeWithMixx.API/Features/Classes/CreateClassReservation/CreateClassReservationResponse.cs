using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Classes.CreateClassReservation;

public record CreateClassReservationResponse
{
    public int Id { get; init; } 
    public decimal PaidAmount { get; init; }
    public decimal TotalPrice { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public IReadOnlyList<ClassDto> Classes { get; init; } = []; 

    public record ClassDto
    {
        public int Id { get; init; }
        public int SubjectId { get; init; }
        public decimal Price { get; init; }
        public DateTime StartsAt { get; init; }
        public DateTime EndsAt { get; init; }
    }
}