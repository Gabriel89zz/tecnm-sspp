using Microsoft.EntityFrameworkCore;

namespace TecNM.Api.Core.Http;

public sealed record PagedRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Sort = null)
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize < 1 ? 20 : (PageSize > 100 ? 100 : PageSize);
    public string? Search { get; init; } = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
    public string? Sort { get; init; } = string.IsNullOrWhiteSpace(Sort) ? null : Sort.Trim();
}

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public sealed record OptionItem(
    long Id,
    string Label,
    IReadOnlyDictionary<string, object?>? Meta = null);

public static class PagingExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(items, page, pageSize, totalItems, totalPages);
    }
}
