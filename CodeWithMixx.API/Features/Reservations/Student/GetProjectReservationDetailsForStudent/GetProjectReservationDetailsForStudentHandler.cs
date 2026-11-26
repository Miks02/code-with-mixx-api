using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Reservations.Student.GetProjectReservationDetailsForStudent;

public class GetProjectReservationDetailsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetProjectReservationDetailsForStudentRequest, Result<GetProjectReservationDetailsForStudentResponse>>
{
    public async Task<Result<GetProjectReservationDetailsForStudentResponse>> HandleAsync(
        GetProjectReservationDetailsForStudentRequest request,
        CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();

        var reservation = await context.Reservations
            .Where(r => r.Id == request.ReservationId
                        && r.StudentId == studentId
                        && r.ReservationType == ReservationType.Project)
            .Select(r => new GetProjectReservationDetailsForStudentResponse
            {
                Id = r.Id,
                AdminId = r.AdminId,
                AdminFullName = r.Admin.User.FirstName + " " + r.Admin.User.LastName,
                AdminEmail = r.Admin.User.Email!,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                Description = r.Description,
                ReservedAt = r.ReservedAt,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Projects = r.Projects
                    .OrderBy(p => p.StartDate)
                    .Select(p => new GetProjectReservationDetailsForStudentResponse.ProjectDetailsItem
                    {
                        Id = p.Id,
                        Name = p.Name,
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
                            .Select(n => new GetProjectReservationDetailsForStudentResponse.ProjectNoteItem
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
            return Result<GetProjectReservationDetailsForStudentResponse>.Failure(ReservationError.NotFound(request.ReservationId));

        return Result<GetProjectReservationDetailsForStudentResponse>.Success(reservation);
    }
}
