using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.Admin.GetOpenBalanceForAdmin;

public class GetOpenBalanceForAdminHandler(AppDbContext context)
    : IHandler<GetOpenBalanceForAdminRequest, GetOpenBalanceForAdminResponse>
{
    public async Task<GetOpenBalanceForAdminResponse> HandleAsync(GetOpenBalanceForAdminRequest request, CancellationToken ct = default)
    {
        var stats = await context.Reservations
            .GroupBy(r => 1)
            .Select(g => new
            {
                RemainingDebt = g.Sum(r => r.ReservationStatus != ReservationStatus.Cancelled && r.TotalPrice > r.PaidAmount
                    ? r.TotalPrice - r.PaidAmount
                    : 0),
                OverdueReservationsCount = g.Count(r => r.PaymentStatus == PaymentStatus.Overdue)
            })
            .FirstOrDefaultAsync(ct);

        var openBalanceItems = await context.Reservations
            .Where(r => r.ReservationStatus != ReservationStatus.Cancelled &&
                        r.TotalPrice > r.PaidAmount)
            .OrderBy(r => r.ReservedAt)
            .Select(r => new GetOpenBalanceForAdminResponse.OpenBalanceItem
            {
                ReservationId = r.Id,
                StudentId = r.StudentId,
                StudentFullName = r.Student.User.FirstName + " " + r.Student.User.LastName,
                ReservationType = r.ReservationType,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                RemainingAmount = r.TotalPrice - r.PaidAmount,
                PaymentStatus = r.PaymentStatus
            })
            .ToListAsync(ct);

        return new GetOpenBalanceForAdminResponse
        {
            RemainingDebt = stats?.RemainingDebt ?? 0,
            OverdueReservationsCount = stats?.OverdueReservationsCount ?? 0,
            OpenBalanceItems = openBalanceItems
        };
    }
}
