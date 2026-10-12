using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.GetStudentsLookup;

public class GetStudentsLookupHandler(AppDbContext context) : IHandler<GetStudentsLookupRequest, IReadOnlyList<GetStudentsLookupResponse>>
{
    public async Task<IReadOnlyList<GetStudentsLookupResponse>> HandleAsync(GetStudentsLookupRequest request, CancellationToken ct = default)
    {
        return await context.Students
            .Select(s => new GetStudentsLookupResponse
            {
                Id = s.UserId,
                FullName = s.User.FirstName + " " + s.User.LastName,
                Email = s.User.Email!
            })
            .ToListAsync(ct);
    }
}
