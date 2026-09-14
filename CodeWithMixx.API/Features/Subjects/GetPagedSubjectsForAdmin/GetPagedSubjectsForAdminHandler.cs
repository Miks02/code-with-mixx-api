using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public class GetPagedSubjectsForAdminHandler(AppDbContext context) : IHandler<GetPagedSubjectsForAdminRequest, GetPagedSubjectsForAdminResponse>
{
    public async Task<GetPagedSubjectsForAdminResponse> HandleAsync(GetPagedSubjectsForAdminRequest request, CancellationToken ct = default)
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
            .Select(s => new GetPagedSubjectsForAdminResponse.SubjectItem
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

        var pagedSubjects = await PagedResult<GetPagedSubjectsForAdminResponse.SubjectItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);

        return new GetPagedSubjectsForAdminResponse
        {
            PagedSubjects = pagedSubjects
        };
    }
}
