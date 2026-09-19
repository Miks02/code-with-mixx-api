using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.RestoreClass;

public class RestoreClassHandler(AppDbContext context) : IHandler<RestoreClassRequest, Result>
{
    public async Task<Result> HandleAsync(RestoreClassRequest request, CancellationToken ct = default)
    {
        var @class = await context.Classes
            .IgnoreQueryFilters()
            .Include(c => c.Reservation)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, ct);

        if (@class is null)
            return Result.Failure(ClassError.NotFound(request.ClassId));

        if (@class.Reservation.IsDeleted)
            return Result.Failure(ClassError.ReservationArchived(@class.Id, @class.ReservationId));

        var restoreResult = @class.Restore();
        if (!restoreResult.IsSuccess)
            return restoreResult;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
