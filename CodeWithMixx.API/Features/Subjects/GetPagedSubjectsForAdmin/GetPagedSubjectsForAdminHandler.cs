using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjectsForAdmin;

public class GetPagedSubjectsForAdminHandler(AppDbContext context) : IHandler<GetPagedSubjectsForAdminRequest, PagedResult<SubjectItem>>
{
    public async Task<PagedResult<SubjectItem>> HandleAsync(GetPagedSubjectsForAdminRequest request, CancellationToken ct = default)
    {
        var subjectsQuery = context.Subjects.AsQueryable();

        if (request.OnlyDeleted)
            subjectsQuery = subjectsQuery.IgnoreQueryFilters().Where(s => s.IsDeleted == true);
            
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            subjectsQuery = subjectsQuery
                .Where(s => EF.Functions.ILike(s.Name, $"%{search}%")
                            || EF.Functions.ILike(s.Description, $"%{search}%"))
                .OrderByDescending(s => s.Name);
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
                ClassesCount = s.Classes.Count,
                StudentsCount = s.Classes
                    .Select(c => c.Reservation.Student)
                    .Distinct()
                    .Count(),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                DeletedAt = s.DeletedAt
            });

        var pagedSubjects = await PagedResult<SubjectItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);
        
        return pagedSubjects;
    }
}
