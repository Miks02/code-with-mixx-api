using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Features.Students.Common;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public class GetPagedStudentsHandler(AppDbContext context) : IHandler<GetPagedStudentsRequest, PagedResult<GetPagedStudentsResponse>>
{
    public async Task<PagedResult<GetPagedStudentsResponse>> HandleAsync(GetPagedStudentsRequest request, CancellationToken ct = default)
    {
        var studentsQuery = context.Students
            .AsQueryable();
        
        if(request.IncludeDeleted)
            studentsQuery = studentsQuery
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted == true);

        studentsQuery = request.SortBy switch
        {
            StudentsSortBy.CreatedAtAscending => studentsQuery.OrderBy(s => s.User.CreatedAt),
            StudentsSortBy.CreatedAtDescending => studentsQuery.OrderByDescending(s => s.User.CreatedAt),
            StudentsSortBy.StudentNameAscending => studentsQuery.OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName),
            StudentsSortBy.StudentNameDescending => studentsQuery.OrderByDescending(s => s.User.FirstName)
                .ThenByDescending(s => s.User.LastName),
            StudentsSortBy.TotalReservationAscending => studentsQuery.OrderBy(s => s.Reservations.Count),
            StudentsSortBy.TotalReservationDescending => studentsQuery.OrderByDescending(s => s.Reservations.Count),
            _ => studentsQuery.OrderByDescending(s => s.User.CreatedAt),
        };

        var invalidFilters = StudentsFilterResolver.GetInvalidFilters(request.Filters);

        foreach (var filter in request.Filters.Except(invalidFilters))
        {
            studentsQuery = filter switch
            {
                StudentsFilterBy.WithClasses => studentsQuery.Where(s => s.Reservations.Any(r => r.Classes.Count != 0)),
                StudentsFilterBy.WithoutClasses => studentsQuery.Where(s => s.Reservations.All(r => r.Classes.Count == 0)),
                StudentsFilterBy.WithProjects => studentsQuery.Where(s => s.Reservations.Any(r => r.Projects.Count != 0)),
                StudentsFilterBy.WithoutProjects => studentsQuery.Where(s => s.Reservations.All(r => r.Projects.Count == 0)),
                _ => studentsQuery
            };
        }

        if(!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            studentsQuery = studentsQuery.Where(s => EF.Functions.ILike(s.User.FirstName, $"%{searchTerm}%")
                                                     || EF.Functions.ILike(s.User.LastName, $"%{searchTerm}%")
                                                     || EF.Functions.ILike(s.User.Email!, $"%{searchTerm}%"));
        }

        var pagedStudents = studentsQuery
            .Select(s => new GetPagedStudentsResponse
            {
                Id = s.UserId,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                Email = s.User.Email!,
                PhoneNumber = s.User.PhoneNumber!,
                University = s.University,
                TotalClasses  = s.Reservations.Sum(r => r.Classes.Count),
                TotalProjects = s.Reservations.Sum(r => r.Projects.Count),
                TotalReservations = s.Reservations.Count,
                RegisteredAt = s.User.CreatedAt,
                DeletedAt = s.DeletedAt,
            });
        
        return await PagedResult<GetPagedStudentsResponse>.CreateAsync(pagedStudents, request.PageNumber, request.PageSize, ct);
    }
}
