namespace CodeWithMixx.API.Domain.Entities.Reservations;

public record ClassCreateData
{
    public int SubjectId { get; init; }
    public decimal Price { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
}