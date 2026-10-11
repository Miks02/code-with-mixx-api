using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetSubjectsLookup;

public class GetSubjectsLookupHandler(AppDbContext context) : IHandler<GetSubjectsLookupRequest, IReadOnlyList<GetSubjectsLookupResponse>>
{
    public async Task<IReadOnlyList<GetSubjectsLookupResponse>> HandleAsync(GetSubjectsLookupRequest request, CancellationToken ct = default)
    {
        return await context.Subjects
            .Select(s => new GetSubjectsLookupResponse
            {
                Id = s.Id,
                SubjectName = s.Name
            })
            .ToListAsync(ct);
    }
}