using CodeWithMixx.API.Domain.Entities.Reservations;

namespace CodeWithMixx.API.Features.Reservations.Student.GetOpenBalanceForStudent;

public record GetOpenBalanceForStudentResponse
{
    public decimal RemainingDebt { get; init; }
    public int OverdueReservationsCount { get; init; }
    public IReadOnlyList<OpenBalanceItem> OpenBalanceItems { get; init; } = [];

    public record OpenBalanceItem
    {
        public int ReservationId { get; init; }
        public ReservationType ReservationType { get; init; }
        public decimal TotalPrice { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal RemainingAmount { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
    }
}
