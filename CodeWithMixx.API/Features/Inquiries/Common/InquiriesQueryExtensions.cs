using CodeWithMixx.API.Domain.Entities.Inquiries;
using Microsoft.EntityFrameworkCore;

namespace CodeWithMixx.API.Features.Inquiries.Common;

public static class InquiriesQueryExtensions
{
    public static IQueryable<Inquiry> ApplyFilters(this IQueryable<Inquiry> query, IReadOnlyCollection<InquiriesFilterBy> filters)
    {
        var resolved = filters.Contains(InquiriesFilterBy.Resolved);
        var unresolved = filters.Contains(InquiriesFilterBy.Unresolved);

        if (resolved != unresolved)
            query = resolved
                ? query.Where(i => i.ResolvedAt != null)
                : query.Where(i => i.ResolvedAt == null);

        var types = filters
            .Select(f => f switch
            {
                InquiriesFilterBy.IndividualClasses => InquiryType.IndividualClasses,
                InquiriesFilterBy.ClassPackage => InquiryType.ClassPackage,
                InquiriesFilterBy.Project => InquiryType.Project,
                InquiriesFilterBy.Other => InquiryType.Other,
                _ => (InquiryType?)null
            })
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .Distinct()
            .ToList();

        if (types.Count > 0)
            query = query.Where(i => types.Contains(i.InquiryType));

        return query;
    }

    public static IQueryable<Inquiry> ApplySearch(this IQueryable<Inquiry> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var pattern = $"%{searchTerm.Trim()}%";

        return query.Where(i => EF.Functions.ILike(i.FirstName + " " + i.LastName, pattern)
                                || EF.Functions.ILike(i.Email, pattern)
                                || EF.Functions.ILike(i.PhoneNumber, pattern)
                                || EF.Functions.ILike(i.Message, pattern));
    }

    public static IQueryable<Inquiry> ApplySort(this IQueryable<Inquiry> query, InquiriesSortBy? sortBy)
    {
        return sortBy switch
        {
            InquiriesSortBy.CreatedAtAscending => query.OrderBy(i => i.CreatedAt).ThenBy(i => i.Id),
            InquiriesSortBy.ResolvedAtAscending => query.OrderBy(i => i.ResolvedAt == null)
                .ThenBy(i => i.ResolvedAt)
                .ThenBy(i => i.Id),
            InquiriesSortBy.ResolvedAtDescending => query.OrderBy(i => i.ResolvedAt == null)
                .ThenByDescending(i => i.ResolvedAt)
                .ThenByDescending(i => i.Id),
            InquiriesSortBy.NameAscending => query.OrderBy(i => i.FirstName)
                .ThenBy(i => i.LastName)
                .ThenBy(i => i.Id),
            InquiriesSortBy.NameDescending => query.OrderByDescending(i => i.FirstName)
                .ThenByDescending(i => i.LastName)
                .ThenByDescending(i => i.Id),
            _ => query.OrderByDescending(i => i.CreatedAt).ThenByDescending(i => i.Id),
        };
    }

    public static IQueryable<InquiryItem> ToInquiryItems(this IQueryable<Inquiry> query)
    {
        return query.Select(i => new InquiryItem
        {
            Id = i.Id,
            FirstName = i.FirstName,
            LastName = i.LastName,
            Email = i.Email,
            PhoneNumber = i.PhoneNumber,
            Message = i.Message,
            InquiryType = i.InquiryType,
            IsResolved = i.ResolvedAt != null,
            ResolvedAt = i.ResolvedAt,
            ResolvedByAdminId = i.ResolvedByAdmin,
            ResolvedByAdminName = i.Admin != null
                ? i.Admin.User.FirstName + " " + i.Admin.User.LastName
                : null,
            CreatedAt = i.CreatedAt,
        });
    }
}
