using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Common.Results;

public class PagedResult<T>(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int PaginatedCount => Items.Count;
    public int TotalCount { get; } = totalCount;
    
    public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> query, int? pageNumber, int? pageSize,
        CancellationToken ct = default)
    {
        if(!pageNumber.HasValue || pageNumber.Value < 1)
            pageNumber = 1;
        
        if(!pageSize.HasValue || pageSize.Value < 1)
            pageSize = 1;
        
        if (pageSize > 100)
            pageSize = 100;

        var totalCount = await query.CountAsync(ct);
        
        if(totalCount == 0)
            return new PagedResult<T>([], pageNumber.Value, pageSize.Value, totalCount);
        
        return new PagedResult<T>(await query.Skip((pageNumber.Value - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .ToListAsync(ct), pageNumber.Value, pageSize.Value, totalCount);
    }

}