using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.SearchStudents
{
    public class SearchStudentsHandler(AppDbContext context)
        : IHandler<SearchStudentsRequest, SearchStudentsResponse>
    {
        public async Task<SearchStudentsResponse> HandleAsync(SearchStudentsRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Search))
                return new SearchStudentsResponse();

            var search = request.Search.Trim();

            var students = await context.Students
                .Where(s => EF.Functions.ILike(s.User.FirstName, $"%{search}%")
                            || EF.Functions.ILike(s.User.LastName, $"%{search}%")
                            || EF.Functions.ILike(s.User.Email!, $"%{search}%"))
                .Select(s => new SearchStudentsResponse.StudentDto
                {
                    Id = s.UserId,
                    FullName = $"{s.User.FirstName} {s.User.LastName}",
                    Email = s.User.Email!,
                    University = s.University
                })
                .ToListAsync(ct);   

            return new SearchStudentsResponse
            {
                Students = students
            };
        }
    }
}
