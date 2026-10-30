using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.CreateClassReservation;

public record CreateClassReservationRequest
{
    public string StudentId { get; init; } = null!;
    public decimal? TotalPrice { get; init; }
    public ReservationStatus ReservationStatus { get; init; }
    public string? Description { get; init; }
    public DateOnly ReservedAt { get; init; }
    
    public IReadOnlyList<ClassItemRequest> Classes { get; init; } = []; 

    public record ClassItemRequest
    {
        public int SubjectId { get; init; }
        public decimal Price { get; init; }
        public DateTime StartsAt { get; init; }
        public DateTime EndsAt { get; init; }
        public DateOnly ReservedAt { get; init; }
    }
};