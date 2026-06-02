using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models.Dto;

namespace PersonalFinanceTracker;

public static class PaginationExtensions
{
    public static async Task<PagedResponse<TResponse>> ToPagedResponseAsync<TSource, TResponse>(
        this IQueryable<TSource> query,
        int page,
        int pageSize,
        Func<TSource, TResponse> map)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var pagination = new PaginationMetadata(
            page,
            pageSize,
            totalItems,
            totalPages,
            page > 1,
            page < totalPages);

        return new PagedResponse<TResponse>(items.Select(map).ToList(), pagination);
    }
}
