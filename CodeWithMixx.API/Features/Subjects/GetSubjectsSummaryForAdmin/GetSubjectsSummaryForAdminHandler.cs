using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsSummaryForAdmin;

public class GetSubjectsSummaryForAdminHandler(AppDbContext context) : IHandler<GetSubjectsSummaryForAdminRequest, GetSubjectsSummaryForAdminResponse>
{
    public async Task<GetSubjectsSummaryForAdminResponse> HandleAsync(GetSubjectsSummaryForAdminRequest request, CancellationToken ct = default)
    {
        var subjectsQuery = context.Subjects.AsQueryable();
        
        subjectsQuery = request.SortBy switch
        {
            SubjectsSortBy.CreatedAtDescending => subjectsQuery.OrderByDescending(s => s.CreatedAt),
            SubjectsSortBy.CreatedAtAscending => subjectsQuery.OrderBy(s => s.CreatedAt),
            SubjectsSortBy.SubjectNameAscending => subjectsQuery.OrderBy(s => s.Name),
            SubjectsSortBy.SubjectNameDescending => subjectsQuery.OrderByDescending(s => s.Name),
            _ => subjectsQuery
        };
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            subjectsQuery = subjectsQuery
                .Where(s => EF.Functions.ILike(s.Name, $"%{search}%")
                            || EF.Functions.ILike(s.Description, $"%{search}%"))
                .OrderByDescending(s => s.Name);
        }
        
        var projectedPageQuery = subjectsQuery
            .Select(s => new GetSubjectsSummaryForAdminResponse.SubjectItem
            {
                Id = s.Id,
                SubjectName = s.Name,
                SubjectDescription = s.Description,
                ClassesCount = s.Classes.Count,
                StudentsCount = s.Classes
                    .Select(c => c.Reservation.Student)
                    .Distinct()
                    .Count(),
                CreatedAt = s.CreatedAt
            });
        
        var pagedSubjects = await PagedResult<GetSubjectsSummaryForAdminResponse.SubjectItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);
        
        var mostPopularSubject = await context.Subjects
            .Select(s => new GetSubjectsSummaryForAdminResponse.SubjectItem
            {
                Id = s.Id,
                SubjectName = s.Name,
                SubjectDescription = s.Description,
                ClassesCount = s.Classes.Count,
                StudentsCount = s.Classes.Select(c => c.Reservation.Student)
                    .Distinct()
                    .Count(),
                CreatedAt = s.CreatedAt
            })
            .OrderByDescending(s => s.ClassesCount)
            .ThenByDescending(s => s.StudentsCount)
            .FirstOrDefaultAsync(ct);

        var stats = await context.Subjects
            .GroupBy(s => 1)
            .Select(g => new
            {
                TotalSubjects = g.Count(),
                TaughtSubjects = g.Count(s => s.Classes.Count != 0),
                UntaughtSubjects = g.Count(s => s.Classes.Count == 0)
            })
            .FirstOrDefaultAsync(ct);
        
        var statsResult = stats ?? new
        {
            TotalSubjects = 0,
            TaughtSubjects = 0,
            UntaughtSubjects = 0
        };
        
        return new GetSubjectsSummaryForAdminResponse
        {
            PagedSubjects = pagedSubjects,
            TotalSubjects = statsResult.TotalSubjects,
            TaughtSubjects = statsResult.TaughtSubjects,
            UntaughtSubjects = statsResult.UntaughtSubjects,
            MostPopularSubject = mostPopularSubject
        };
    }
}