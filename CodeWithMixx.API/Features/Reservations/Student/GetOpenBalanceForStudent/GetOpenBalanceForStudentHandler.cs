using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.Student.GetOpenBalanceForStudent;

public class GetOpenBalanceForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetOpenBalanceForStudentRequest, GetOpenBalanceForStudentResponse>
{
    public async Task<GetOpenBalanceForStudentResponse> HandleAsync(GetOpenBalanceForStudentRequest request, CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var studentReservations = context.Reservations
            .Where(r => r.StudentId == studentId);

        var stats = await studentReservations
            .GroupBy(r => 1)
            .Select(g => new
            {
                RemainingDebt = g.Sum(r => r.ReservationStatus != ReservationStatus.Cancelled && r.TotalPrice > r.PaidAmount
                    ? r.TotalPrice - r.PaidAmount
                    : 0),
                OverdueReservationsCount = g.Count(r => r.PaymentStatus == PaymentStatus.Overdue)
            })
            .FirstOrDefaultAsync(ct);

        var openBalanceItems = await studentReservations
            .Where(r => r.ReservationStatus != ReservationStatus.Cancelled &&
                        r.TotalPrice > r.PaidAmount)
            .OrderBy(r => r.ReservedAt)
            .Select(r => new GetOpenBalanceForStudentResponse.OpenBalanceItem
            {
                ReservationId = r.Id,
                ReservationType = r.ReservationType,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                RemainingAmount = r.TotalPrice - r.PaidAmount,
                PaymentStatus = r.PaymentStatus
            })
            .ToListAsync(ct);

        return new GetOpenBalanceForStudentResponse
        {
            RemainingDebt = stats?.RemainingDebt ?? 0,
            OverdueReservationsCount = stats?.OverdueReservationsCount ?? 0,
            OpenBalanceItems = openBalanceItems
        };
    }
}
