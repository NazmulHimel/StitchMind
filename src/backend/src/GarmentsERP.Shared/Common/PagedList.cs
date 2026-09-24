namespace GarmentsERP.Shared.Common;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items; Page = page; PageSize = pageSize; TotalCount = totalCount;
    }

    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var list = source.ToList();
        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, page, pageSize, total);
    }
}
