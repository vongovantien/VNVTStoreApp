namespace VNVTStore.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalItems { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;

    public PagedResult(IEnumerable<T> items, int totalItems, int pageIndex = 1, int pageSize = 10)
    {
        Items = items;
        TotalItems = totalItems;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty() =>
        new(Enumerable.Empty<T>(), 0, 1, 10);
}
