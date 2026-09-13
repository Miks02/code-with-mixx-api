namespace CodeWithMixx.API.Domain.Entities.Reservations;

public record ReservationCreateData
{
    public string StudentId { get; init; } = null!;
    public string AdminId { get; init; } = null!;
    public ReservationStatus ReservationStatus { get; init; } = ReservationStatus.Confirmed;
    public decimal? TotalPrice { get; init; }
    public decimal PaidAmount { get; init; }
    public IReadOnlyList<ClassCreateData> Classes { get; init; } = [];
};