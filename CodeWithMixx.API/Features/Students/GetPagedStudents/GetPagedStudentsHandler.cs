using CodeWithMixx.API.Common.Interfaces;
using CodeWithMixx.API.Common.Results;
using CodeWithMixx.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Students.GetPagedStudents;

public class GetPagedStudentsHandler(AppDbContext context)
    : IHandler<GetPagedStudentsRequest, Result<PagedResult<GetPagedStudentsResponse>>>
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private const int DefaultPageNumber = 1;

    public async Task<Result<PagedResult<GetPagedStudentsResponse>>> HandleAsync(
        GetPagedStudentsRequest request,
        CancellationToken ct = default)
    {
        var pageNumber = request.PageNumber <= 0 ? DefaultPageNumber : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? DefaultPageSize : (request.PageSize > MaxPageSize ? MaxPageSize : request.PageSize);

        var query = context.Students.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query
                .Where(s => EF.Functions.ILike(s.User.FirstName, $"%{search}%")
                            || EF.Functions.ILike(s.User.LastName, $"%{search}%")
                            || EF.Functions.ILike(s.User.Email!, $"%{search}%"))
                .OrderByDescending(s => s.User.LastName);
        }

        var projectedQuery = query.Select(s => new GetPagedStudentsResponse
        {
            Id = s.UserId,
            FirstName = s.User.FirstName,
            LastName = s.User.LastName,
            Email = s.User.Email!,
            University = s.University
        });

        var pagedResult = await PagedResult<GetPagedStudentsResponse>.CreateAsync(projectedQuery, pageNumber, pageSize, ct);

        return Result<PagedResult<GetPagedStudentsResponse>>.Success(pagedResult);
    }
}
