using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Projects.CreateProjectReservation;

public class CreateProjectReservationHandler (AppDbContext context, IUserProvider userProvider)
    : IHandler<CreateProjectReservationRequest, Result<CreateProjectReservationResponse>>
{
    public async Task<Result<CreateProjectReservationResponse>> HandleAsync(CreateProjectReservationRequest request, CancellationToken ct = default)
    {
        var studentExists = await context.Students.AnyAsync(s => s.UserId == request.StudentId, ct);
        
        if (!studentExists)
            return Result<CreateProjectReservationResponse>.Failure(StudentError.NotFound(request.StudentId));
        
        var subjectIds = request.Projects.Select(c => c.SubjectId).ToList();
        
        var subjects = await context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(ct);
        
        if(subjects.Count != subjectIds.Distinct().Count())
        {
            var missingSubjectIds = subjectIds.Except(subjects.Select(s => s.Id)).ToList();
            return Result<CreateProjectReservationResponse>.Failure(SubjectError.MultipleSubjectsMissing(missingSubjectIds));
        }

        var reservationData = new ReservationCreateData
        {
            AdminId = userProvider.GetUserId(),
            StudentId = request.StudentId,
            PaidAmount = request.PaidAmount,
            ReservationStatus = request.ReservationStatus,
            TotalPrice = request.TotalPrice,
        };

        var projectsData = request.Projects
            .Select(p => new ProjectCreateData
            {
                SubjectId = p.SubjectId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ProjectType = p.ProjectType,
                Progress = p.Progress,
                Price = p.Price,
                GithubLink = p.GithubLink,
                DownloadLink = p.DownloadLink,
                ReservedAt = p.ReservedAt,
                Notes = p.Notes.Select(n => new ProjectCreateData.ProjectNoteData
                {
                    Content = n
                }).ToList()
            })
            .ToList();
        
        var createReservationResult = Reservation.CreateProjectReservation(reservationData, projectsData);

        if(!createReservationResult.IsSuccess)
            return Result<CreateProjectReservationResponse>.Failure(createReservationResult.Errors[0]);

        var newReservation = createReservationResult.Payload!;
        
        context.Reservations.Add(newReservation);
        await context.SaveChangesAsync(ct);
        
        var response = new CreateProjectReservationResponse
        {
            Id = newReservation.Id,
            AdminId = newReservation.AdminId,
            StudentId = newReservation.StudentId,
            TotalPrice = newReservation.TotalPrice,
            PaidAmount = newReservation.PaidAmount,
            PaymentStatus = newReservation.PaymentStatus,
            ReservationStatus = newReservation.ReservationStatus,
            CreatedAt = newReservation.CreatedAt,
            Projects = newReservation.Projects.Select(p => new CreateProjectReservationResponse.ProjectItem
            {
                Id = p.Id,
                SubjectId = p.SubjectId,
                SubjectName = p.Subject.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ProjectType = p.ProjectType,
                Progress = p.Progress,
                Price = p.Price,
                CreatedAt = p.CreatedAt,
                ReservedAt = p.ReservedAt,
                DownloadLink = p.DownloadLink,
                GithubLink = p.GithubLink,
                Notes = p.ProjectNotes.Select(n => new CreateProjectReservationResponse.ProjectNoteItem
                {
                    Id = n.Id,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt
                }).ToList()
            }).ToList()
            
        };
        
        return Result<CreateProjectReservationResponse>.Success(response);
    }
}