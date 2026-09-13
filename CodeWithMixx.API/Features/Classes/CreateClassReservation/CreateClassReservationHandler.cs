using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Classes.CreateClassReservation;

public class CreateClassReservationHandler(AppDbContext context, IUserProvider userProvider) : IHandler<CreateClassReservationRequest, Result<CreateClassReservationResponse>> 
{
    public async Task<Result<CreateClassReservationResponse>> HandleAsync(CreateClassReservationRequest request, CancellationToken ct = default)
    {
        var studentExists = await context.Students
            .AnyAsync(s => s.UserId == request.StudentId, ct);

        if (!studentExists)
            return Result<CreateClassReservationResponse>.Failure(StudentError.NotFound(request.StudentId));
        
        var subjectIds = request.Classes.Select(c => c.SubjectId).ToList(); 
        
        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(ct);
        
        if(subjects.Count != subjectIds.Distinct().Count())
        {
            var missingSubjectIds = subjectIds.Except(subjects.Select(s => s.Id)).ToList();
            return Result<CreateClassReservationResponse>.Failure(SubjectError.MultipleSubjectsMissing(missingSubjectIds));
        }

        var classes = request.Classes
            .Select(c => new ClassCreateData
            {
                SubjectId = c.SubjectId,
                Price = c.Price,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt
            })
            .ToList();

        var newReservationData = new ReservationCreateData
        {
            AdminId = userProvider.GetUserId(),
            StudentId = request.StudentId,
            ReservationStatus = request.ReservationStatus,
            TotalPrice = request.TotalPrice,
            PaidAmount = request.PaidAmount,
            Classes = classes
        };
 
        var reservationResult = Reservation.CreateClassReservation(newReservationData);

        if (!reservationResult.IsSuccess)
            return Result<CreateClassReservationResponse>.Failure(reservationResult.Errors[0]);
        
        context.Reservations.Add(reservationResult.Payload!);
        await context.SaveChangesAsync(ct);

        var response = new CreateClassReservationResponse
        {
            Id = reservationResult.Payload!.Id,
            PaymentStatus = reservationResult.Payload.PaymentStatus,
            ReservationStatus = reservationResult.Payload.ReservationStatus,
            TotalPrice = reservationResult.Payload.TotalPrice,
            PaidAmount = reservationResult.Payload.PaidAmount,
            Classes = reservationResult.Payload.Classes.Select(c => new CreateClassReservationResponse.ClassDto
            {
                Id = c.Id,
                SubjectId = c.SubjectId,
                Price = c.Price,
                StartsAt = c.StartsAt,
                EndsAt = c.EndsAt
            }).ToList()
        };
        
        return Result<CreateClassReservationResponse>.Success(response);
    }

}