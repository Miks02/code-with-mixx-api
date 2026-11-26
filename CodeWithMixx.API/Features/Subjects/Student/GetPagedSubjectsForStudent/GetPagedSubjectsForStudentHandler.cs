using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Subjects.Student.Common;
using CodeWithMixx.API.Features.Subjects.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.Student.GetPagedSubjectsForStudent;

public class GetPagedSubjectsForStudentHandler(AppDbContext context, IUserProvider userProvider)
    : IHandler<GetPagedSubjectsForStudentRequest, PagedResult<SubjectItem>>
{
    public async Task<PagedResult<SubjectItem>> HandleAsync(GetPagedSubjectsForStudentRequest request, CancellationToken ct = default)
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

        return await PagedResult<SubjectItem>
            .CreateAsync(projectedPageQuery, request.PageNumber, request.PageSize, ct);
    }
}
