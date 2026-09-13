using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Subjects.GetPagedSubjects;

public class GetPagedSubjectsHandler(AppDbContext context)
    : IHandler<GetPagedSubjectsRequest, Result<PagedResult<GetPagedSubjectsResponse>>>
{

    public async Task<Result<PagedResult<GetPagedSubjectsResponse>>> HandleAsync(
        GetPagedSubjectsRequest request,
        CancellationToken ct = default)
    {
        var query = context.Subjects
            .Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query
                .Where(s => EF.Functions.ILike(s.Name, $"%{search}%")
                            || EF.Functions.ILike(s.Description, $"%{search}%"))
                .OrderByDescending(s => s.Name);
        }

        var projectedQuery = query.Select(s => new GetPagedSubjectsResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description
        });

        var pagedResult = await PagedResult<GetPagedSubjectsResponse>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize, ct);

        return Result<PagedResult<GetPagedSubjectsResponse>>.Success(pagedResult);
    }
}
