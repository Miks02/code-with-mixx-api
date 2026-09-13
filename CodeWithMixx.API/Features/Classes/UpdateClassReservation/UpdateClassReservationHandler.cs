using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Classes;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Features.Classes.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.UpdateClassReservation;

public class UpdateClassReservationHandler(AppDbContext context)
    : IHandler<UpdateClassReservationRequest, Result<UpdateClassReservationResponse>>
{
    public async Task<Result<UpdateClassReservationResponse>> HandleAsync(UpdateClassReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Classes)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (reservation is null)
            return Result<UpdateClassReservationResponse>.Failure(ReservationError.NotFound(request.Id));

        if (!string.Equals(reservation.StudentId, request.StudentId, StringComparison.Ordinal))
        {
            var studentExists = await context.Students.AnyAsync(s => s.UserId == request.StudentId, ct);

            if (!studentExists)
                return Result<UpdateClassReservationResponse>.Failure(StudentError.NotFound(request.StudentId));

            reservation.ChangeStudent(request.StudentId);
        }

        var deleteResult = DeleteClasses(reservation, request.ClassesToDelete);

        if (!deleteResult.IsSuccess)
            return Result<UpdateClassReservationResponse>.Failure(deleteResult.Errors[0]);

        var updateResult = await UpdateExistingClassesAsync(reservation, request.Classes, ct);

        if (!updateResult.IsSuccess)
            return Result<UpdateClassReservationResponse>.Failure(updateResult.Errors[0]);

        var addResult = await AddNewClassesAsync(reservation, request.Classes, ct);

        if (!addResult.IsSuccess)
            return Result<UpdateClassReservationResponse>.Failure(addResult.Errors[0]);

        reservation.UpdateReservationStatus(request.ReservationStatus);
        reservation.UpdateNotes(request.Notes);

        var totalPriceResult = reservation.UpdateTotalPrice(request.TotalPrice);

        if (!totalPriceResult.IsSuccess)
            return Result<UpdateClassReservationResponse>.Failure(totalPriceResult.Errors[0]);

        var paidAmountDelta = request.PaidAmount - reservation.PaidAmount;

        if (paidAmountDelta > 0)
        {
            var paymentResult = reservation.RegisterPayment(paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateClassReservationResponse>.Failure(paymentResult.Errors[0]);
        }
        else if (paidAmountDelta < 0)
        {
            var paymentResult = reservation.SubtractPayment(-paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateClassReservationResponse>.Failure(paymentResult.Errors[0]);
        }

        await context.SaveChangesAsync(ct);

        var response = new UpdateClassReservationResponse
        {
            Id = reservation.Id,
            StudentId = reservation.StudentId,
            ReservationStatus = reservation.ReservationStatus,
            PaymentStatus = reservation.PaymentStatus,
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            DiscountRate = reservation.DiscountRate,
            Bonus = reservation.Bonus,
            Notes = reservation.Notes,
            UpdatedAt = reservation.UpdatedAt
        };

        return Result<UpdateClassReservationResponse>.Success(response);
    }

    private Result DeleteClasses(Reservation reservation, IReadOnlyList<int> classIdsToDelete)
    {
        if (classIdsToDelete.Count == 0)
            return Result.Success();

        var existingClassIds = reservation.Classes.Select(c => c.Id).ToHashSet();
        var invalidClassIds = classIdsToDelete.Where(id => !existingClassIds.Contains(id)).ToList();

        if (invalidClassIds.Count != 0)
            return Result.Failure(ClassError.NotFound(invalidClassIds[0]));

        var classesToRemove = reservation.Classes.Where(c => classIdsToDelete.Contains(c.Id)).ToList();

        foreach (var classToRemove in classesToRemove)
            reservation.Classes.Remove(classToRemove);

        context.Classes.RemoveRange(classesToRemove);

        return Result.Success();
    }

    private async Task<Result> UpdateExistingClassesAsync(Reservation reservation, IReadOnlyList<ClassItem> classes, CancellationToken ct)
    {
        var classesToUpdate = classes.Where(c => c.Id != 0).ToList();

        if (classesToUpdate.Count == 0)
            return Result.Success();

        var subjectsResult = await EnsureSubjectsExistAsync(classesToUpdate.Select(c => c.SubjectId), ct);

        if (!subjectsResult.IsSuccess)
            return subjectsResult;

        var updateData = classesToUpdate
            .Select(c => new ClassUpdateData
            {
                Id = c.Id,
                SubjectId = c.SubjectId,
                Price = c.Price,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt
            })
            .ToList();

        return reservation.UpdateClasses(updateData);
    }

    private async Task<Result> AddNewClassesAsync(Reservation reservation, IReadOnlyList<ClassItem> classes, CancellationToken ct)
    {
        var classesToAdd = classes.Where(c => c.Id == 0).ToList();

        if (classesToAdd.Count == 0)
            return Result.Success();

        var subjectsResult = await EnsureSubjectsExistAsync(classesToAdd.Select(c => c.SubjectId), ct);

        if (!subjectsResult.IsSuccess)
            return subjectsResult;

        var createData = classesToAdd
            .Select(c => new ClassCreateData
            {
                SubjectId = c.SubjectId,
                Price = c.Price,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt
            })
            .ToList();

        return reservation.AddClasses(createData);
    }

    private async Task<Result> EnsureSubjectsExistAsync(IEnumerable<int> subjectIds, CancellationToken ct)
    {
        var distinctSubjectIds = subjectIds.Distinct().ToList();

        var existingSubjectIds = await context.Subjects
            .Where(s => distinctSubjectIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (existingSubjectIds.Count == distinctSubjectIds.Count)
            return Result.Success();

        var missingSubjectIds = distinctSubjectIds.Except(existingSubjectIds).ToList();

        return Result.Failure(SubjectError.MultipleSubjectsMissing(missingSubjectIds));
    }
}
