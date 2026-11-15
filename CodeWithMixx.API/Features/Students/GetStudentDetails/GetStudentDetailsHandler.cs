using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Domain.Entities.Reservations;
using CodeWithMixx.API.Domain.Entities.Students;
using CodeWithMixx.API.Features.Reservations.Common;
using CodeWithMixx.API.Features.Students.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.GetStudentDetails;

public class GetStudentDetailsHandler(AppDbContext context)
    : IHandler<GetStudentDetailsRequest, Result<GetStudentDetailsResponse>>
{
    public async Task<Result<GetStudentDetailsResponse>> HandleAsync(
        GetStudentDetailsRequest request,
        CancellationToken ct = default)
    {
        var student = await context.Students
            .Where(s => s.UserId == request.Id)
            .Select(s => new
            {
                Student = new StudentItem
                {
                    Id = s.UserId,
                    FirstName = s.User.FirstName,
                    LastName = s.User.LastName,
                    Email = s.User.Email!,
                    PhoneNumber = s.User.PhoneNumber,
                    University = s.University,
                    AccountStatus = s.User.AccountStatus,
                    RegisteredAt = s.User.CreatedAt,
                    LastLoginAt = s.User.LastLoginAt,
                    DeletedAt = s.DeletedAt
                },
                Stats = new GetStudentDetailsResponse.ReservationsStats
                {
                    TotalReservations = s.Reservations.Count,
                    ClassReservations = s.Reservations.Count(r => r.ReservationType == ReservationType.Class),
                    ProjectReservations = s.Reservations.Count(r => r.ReservationType == ReservationType.Project),
                    TotalClasses = s.Reservations.Sum(r => r.Classes.Count),
                    TotalProjects = s.Reservations.Sum(r => r.Projects.Count),
                    OverdueReservations = s.Reservations.Count(r => r.PaymentStatus == PaymentStatus.Overdue),
                    TotalPrice = s.Reservations.Sum(r => r.TotalPrice),
                    TotalPaid = s.Reservations.Sum(r => r.PaidAmount),
                    TotalRemaining = s.Reservations.Sum(r => r.TotalPrice > r.PaidAmount ? r.TotalPrice - r.PaidAmount : 0)
                }
            })
            .FirstOrDefaultAsync(ct);

        if (student is null)
            return Result<GetStudentDetailsResponse>.Failure(StudentError.NotFound(request.Id));

        var reservationsQuery = context.Reservations
            .Where(r => r.StudentId == request.Id);

        if (request.ReservationType.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.ReservationType == request.ReservationType.Value);

        var projectedQuery = reservationsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetStudentDetailsResponse.StudentReservation
            {
                Id = r.Id,
                ReservationType = r.ReservationType,
                ReservationStatus = r.ReservationStatus,
                PaymentStatus = r.PaymentStatus,
                TotalPrice = r.TotalPrice,
                PaidAmount = r.PaidAmount,
                DiscountRate = r.DiscountRate,
                Bonus = r.Bonus,
                Description = r.Description,
                ReservedAt = r.ReservedAt,
                CreatedAt = r.CreatedAt,
                Classes = r.Classes
                    .OrderBy(c => c.StartsAt)
                    .Select(c => new ClassItem
                    {
                        Id = c.Id,
                        SubjectId = c.SubjectId,
                        SubjectName = c.Subject.Name,
                        Price = c.Price,
                        StartsAt = c.StartsAt,
                        EndsAt = c.EndsAt,
                        ReservedAt = c.ReservedAt
                    })
                    .ToList(),
                Projects = r.Projects
                    .OrderBy(p => p.StartDate)
                    .Select(p => new ProjectItem
                    {
                        Id = p.Id,
                        SubjectId = p.SubjectId,
                        SubjectName = p.Subject.Name,
                        ProjectType = p.ProjectType,
                        Price = p.Price,
                        Progress = p.Progress,
                        ReservedAt = p.ReservedAt,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        GithubLink = p.GithubLink,
                        DownloadLink = p.DownloadLink,
                        NotesCount = p.ProjectNotes.Count
                    })
                    .ToList()
            });

        var pagedReservations = await PagedResult<GetStudentDetailsResponse.StudentReservation>.CreateAsync(
            projectedQuery, request.PageNumber, request.PageSize, ct);

        return Result<GetStudentDetailsResponse>.Success(new GetStudentDetailsResponse
        {
            Student = student.Student,
            Stats = student.Stats,
            Reservations = pagedReservations
        });
    }
}
