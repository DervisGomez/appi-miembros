using ChurchApi.Dtos;

namespace ChurchApi.Helpers;

public static class PaginationHelper
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) NormalizePaging(int requestedPage, int requestedPageSize)
    {
        var page = requestedPage < 1 ? DefaultPage : requestedPage;
        var pageSize = requestedPageSize < 1 ? DefaultPageSize : requestedPageSize;

        return (page, Math.Min(pageSize, MaxPageSize));
    }

    public static IQueryable<T> ApplyPaging<T>(IQueryable<T> query, int page, int pageSize)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);
    }

    public static PagedResponse<T> BuildPagedResponse<T>(
        List<T> items,
        int page,
        int pageSize,
        int totalItems)
    {
        return new PagedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = CalculateTotalPages(totalItems, pageSize)
        };
    }
}
