using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Common.Results;

public class PagedResult<T>(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
{
    public IReadOnlyList<T> Items { get; init; } = items;
    public int PageNumber { get; init; } = pageNumber;
    public int PageSize { get; init; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int PaginatedCount => Items.Count;
    public int TotalCount { get; init; } = totalCount;

    public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> query, int pageNumber, int pageSize,
        CancellationToken ct = default)
    {
        if (pageSize > 100)
            pageSize = 100;
        
        if(pageSize < 1)
            pageSize = 1;
        
        if(pageNumber < 1)
            pageNumber = 1;

        var totalCount = await query.CountAsync(ct);
        return new PagedResult<T>(await query.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct), pageNumber, pageSize, totalCount);
    }

}