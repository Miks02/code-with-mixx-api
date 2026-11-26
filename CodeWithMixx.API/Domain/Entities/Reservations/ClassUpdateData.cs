namespace CodeWithMixx.API.Domain.Entities.Reservations;

public record ClassUpdateData
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int SubjectId { get; init; }
    public decimal Price { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public DateOnly ReservedAt { get; init; }
}