using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.GetProjectReservationDetails;

public class GetProjectReservationDetailsHandler(AppDbContext context)
    : IHandler<GetProjectReservationDetailsRequest, Result<GetProjectReservationDetailsResponse>>
{
    public async Task<Result<GetProjectReservationDetailsResponse>> HandleAsync(
        GetProjectReservationDetailsRequest request,
        CancellationToken ct = default)
    {
        var reservation = await context.Reservations
            .Where(r => r.Id == request.ReservationId && r.ReservationType == ReservationType.Project)
            .Select(r => new GetProjectReservationDetailsResponse
            {
                Id = r.Id,
                StudentId = r.StudentId,
                StudentFullName = r.Student.User.FirstName + " " + r.Student.User.LastName,
                StudentEmail = r.Student.User.Email!,
                StudentUniversity = r.Student.University,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Projects = r.Projects
                    .OrderBy(p => p.StartDate)
                    .Select(p => new GetProjectReservationDetailsResponse.ProjectDetailsItem
                    {
                        Id = p.Id,
                        SubjectId = p.SubjectId,
                        SubjectName = p.Subject.Name,
                        ProjectType = p.ProjectType,
                        Price = p.Price,
                        Progress = p.Progress,
                        GithubLink = p.GithubLink,
                        DownloadLink = p.DownloadLink,
                        ReservedAt = p.ReservedAt,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Notes = p.ProjectNotes
                            .OrderBy(n => n.CreatedAt)
                            .Select(n => new GetProjectReservationDetailsResponse.ProjectNoteItem
                            {
                                Id = n.Id,
                                Content = n.Content,
                                CreatedAt = n.CreatedAt,
                                UpdatedAt = n.UpdatedAt
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (reservation is null)
            return Result<GetProjectReservationDetailsResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        return Result<GetProjectReservationDetailsResponse>.Success(reservation);
    }
}
