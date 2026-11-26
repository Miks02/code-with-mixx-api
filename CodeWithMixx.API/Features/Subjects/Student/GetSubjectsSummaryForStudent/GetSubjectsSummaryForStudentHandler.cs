using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Student.Common;
using CodeWithMixx.API.Features.Subjects.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.Student.GetSubjectsSummaryForStudent;

public class GetSubjectsSummaryForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetSubjectsSummaryForStudentRequest, GetSubjectsSummaryForStudentResponse>
{
    public async Task<GetSubjectsSummaryForStudentResponse> HandleAsync(GetSubjectsSummaryForStudentRequest request, CancellationToken ct = default)
    {
        var studentId = userProvider.GetUserId();
        var subjectsQuery = context.Subjects.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            subjectsQuery = subjectsQuery
                .Where(s => EF.Functions.ILike(s.Name, $"%{search}%")
                            || EF.Functions.ILike(s.Description, $"%{search}%"));
        }

        subjectsQuery = request.SortBy switch
        {
            SubjectsSortBy.CreatedAtDescending => subjectsQuery.OrderByDescending(s => s.CreatedAt),
            SubjectsSortBy.CreatedAtAscending => subjectsQuery.OrderBy(s => s.CreatedAt),
            SubjectsSortBy.SubjectNameAscending => subjectsQuery.OrderBy(s => s.Name),
            SubjectsSortBy.SubjectNameDescending => subjectsQuery.OrderByDescending(s => s.Name),
            _ => subjectsQuery.OrderBy(s => s.CreatedAt)
        };

        var projectedPageQuery = subjectsQuery
            .Select(s => new SubjectItem
            {
                Id = s.Id,
                SubjectName = s.Name,
                SubjectDescription = s.Description,
                ClassesCount = s.Classes.Count(c => c.Reservation.StudentId == studentId)
            });

        var pagedSubjects = await PagedResult<SubjectItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);

        var mostPopularSubject = await context.Subjects
            .Select(s => new SubjectItem
            {
                Id = s.Id,
                SubjectName = s.Name,
                SubjectDescription = s.Description,
                ClassesCount = s.Classes.Count(c => c.Reservation.StudentId == studentId)
            })
            .Where(s => s.ClassesCount > 0)
            .OrderByDescending(s => s.ClassesCount)
            .FirstOrDefaultAsync(ct);

        var stats = await context.Subjects
            .GroupBy(s => 1)
            .Select(g => new
            {
                TotalSubjects = g.Count(),
                TaughtSubjects = g.Count(s => s.Classes.Any(c => c.Reservation.StudentId == studentId)),
                UntaughtSubjects = g.Count(s => !s.Classes.Any(c => c.Reservation.StudentId == studentId))
            })
            .FirstOrDefaultAsync(ct);
        
        var lastSubjectClass = await context.Classes
            .Where(c => c.Reservation.StudentId == studentId)
            .OrderByDescending(c => c.StartsAt)
            .Select(c => new GetSubjectsSummaryForStudentResponse.LastSubjectClassItem
            {
                SubjectName = c.Subject.Name,
                ClassDate = DateOnly.FromDateTime(c.StartsAt)
            })
            .FirstOrDefaultAsync(ct);

        return new GetSubjectsSummaryForStudentResponse
        {
            PagedSubjects = pagedSubjects,
            TotalSubjects = stats?.TotalSubjects ?? 0,
            TaughtSubjects = stats?.TaughtSubjects ?? 0,
            UntaughtSubjects = stats?.UntaughtSubjects ?? 0,
            MostPopularSubject = mostPopularSubject,
            LastSubjectClass = lastSubjectClass
        };
    }
}
