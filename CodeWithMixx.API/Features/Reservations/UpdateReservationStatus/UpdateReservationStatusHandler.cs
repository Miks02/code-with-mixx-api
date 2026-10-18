using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.UpdateReservationStatus;

public class UpdateReservationStatusHandler(AppDbContext context) : IHandler<UpdateReservationStatusRequest, Result>
{
    public async Task<Result> HandleAsync(UpdateReservationStatusRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, ct);

        if (reservation is null)
            return Result.Failure(ReservationError.NotFound(request.ReservationId));

        var updateResult = reservation.UpdateReservationStatus(request.Body.ReservationStatus);

        if (!updateResult.IsSuccess)
            return updateResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
