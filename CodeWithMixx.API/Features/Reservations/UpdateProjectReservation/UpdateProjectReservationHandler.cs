using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Projects;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Domain.Entities.Subjects;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.UpdateProjectReservation;

public class UpdateProjectReservationHandler(AppDbContext context)
    : IHandler<UpdateProjectReservationRequest, Result<UpdateProjectReservationResponse>>
{
    public async Task<Result<UpdateProjectReservationResponse>> HandleAsync(UpdateProjectReservationRequest request, CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Include(r => r.Projects)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

        if (reservation is null)
            return Result<UpdateProjectReservationResponse>.Failure(ReservationError.NotFound(request.Id));

        if (!string.Equals(reservation.StudentId, request.StudentId, StringComparison.Ordinal))
        {
            var studentExists = await context.Students.AnyAsync(s => s.UserId == request.StudentId, ct);

            if (!studentExists)
                return Result<UpdateProjectReservationResponse>.Failure(StudentError.NotFound(request.StudentId));

            reservation.ChangeStudent(request.StudentId);
        }

        var deleteResult = DeleteProjects(reservation, request.ProjectsToDelete);

        if (!deleteResult.IsSuccess)
            return Result<UpdateProjectReservationResponse>.Failure(deleteResult.Errors[0]);

        var updateResult = await UpdateExistingProjectsAsync(reservation, request.Projects, ct);

        if (!updateResult.IsSuccess)
            return Result<UpdateProjectReservationResponse>.Failure(updateResult.Errors[0]);

        var addResult = await AddNewProjectsAsync(reservation, request.Projects, ct);

        if (!addResult.IsSuccess)
            return Result<UpdateProjectReservationResponse>.Failure(addResult.Errors[0]);

        reservation.UpdateReservationStatus(request.ReservationStatus);
        reservation.UpdateNotes(request.Notes);

        var totalPriceResult = reservation.UpdateTotalPrice(request.TotalPrice);

        if (!totalPriceResult.IsSuccess)
            return Result<UpdateProjectReservationResponse>.Failure(totalPriceResult.Errors[0]);

        var paidAmountDelta = request.PaidAmount - reservation.PaidAmount;

        if (paidAmountDelta > 0)
        {
            var paymentResult = reservation.RegisterPayment(paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateProjectReservationResponse>.Failure(paymentResult.Errors[0]);
        }
        else if (paidAmountDelta < 0)
        {
            var paymentResult = reservation.SubtractPayment(-paidAmountDelta);

            if (!paymentResult.IsSuccess)
                return Result<UpdateProjectReservationResponse>.Failure(paymentResult.Errors[0]);
        }

        await context.SaveChangesAsync(ct);

        var response = new UpdateProjectReservationResponse
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

        return Result<UpdateProjectReservationResponse>.Success(response);
    }

    private Result DeleteProjects(Reservation reservation, IReadOnlyList<int> projectIdsToDelete)
    {
        if (projectIdsToDelete.Count == 0)
            return Result.Success();

        var existingProjectIds = reservation.Projects.Select(p => p.Id).ToHashSet();
        var invalidProjectIds = projectIdsToDelete.Where(id => !existingProjectIds.Contains(id)).ToList();

        if (invalidProjectIds.Count != 0)
            return Result.Failure(ProjectError.NotFound(invalidProjectIds[0]));

        var projectsToRemove = reservation.Projects.Where(p => projectIdsToDelete.Contains(p.Id)).ToList();

        foreach (var projectToRemove in projectsToRemove)
            reservation.Projects.Remove(projectToRemove);

        context.Projects.RemoveRange(projectsToRemove);

        return Result.Success();
    }

    private async Task<Result> UpdateExistingProjectsAsync(Reservation reservation, IReadOnlyList<UpdateProjectReservationRequest.ProjectItem> projects, CancellationToken ct)
    {
        var projectsToUpdate = projects.Where(p => p.Id != 0).ToList();

        if (projectsToUpdate.Count == 0)
            return Result.Success();

        var subjectsResult = await EnsureSubjectsExistAsync(projectsToUpdate.Select(p => p.SubjectId), ct);

        if (!subjectsResult.IsSuccess)
            return subjectsResult;

        var updateData = projectsToUpdate
            .Select(p => new ProjectUpdateData
            {
                Id = p.Id,
                SubjectId = p.SubjectId,
                ProjectType = p.ProjectType,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ReservedAt = p.ReservedAt,
                GithubLink = p.GithubLink,
                DownloadLink = p.DownloadLink,
                Price = p.Price,
                Progress = p.Progress
            })
            .ToList();

        return reservation.UpdateProjects(updateData);
    }

    private async Task<Result> AddNewProjectsAsync(Reservation reservation, IReadOnlyList<UpdateProjectReservationRequest.ProjectItem> projects, CancellationToken ct)
    {
        var projectsToAdd = projects.Where(p => p.Id == 0).ToList();

        if (projectsToAdd.Count == 0)
            return Result.Success();

        var subjectsResult = await EnsureSubjectsExistAsync(projectsToAdd.Select(p => p.SubjectId), ct);

        if (!subjectsResult.IsSuccess)
            return subjectsResult;

        var createData = projectsToAdd
            .Select(p => new ProjectCreateData
            {
                SubjectId = p.SubjectId,
                ProjectType = p.ProjectType,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ReservedAt = p.ReservedAt,
                GithubLink = p.GithubLink,
                DownloadLink = p.DownloadLink,
                Price = p.Price,
                Progress = p.Progress,
                Notes = p.Notes.Select(n => new ProjectCreateData.ProjectNoteData
                {
                    Content = n
                }).ToList()
            })
            .ToList();

        return reservation.AddProjects(createData);
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
