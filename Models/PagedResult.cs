namespace BasicStackOverflow.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalPages { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }

    public PagedResult(List<T> items, int totalItems, int pageSize, int currentPage)
    {
        Items = items;
        TotalItems = totalItems;
        CurrentPage = currentPage;
        TotalPages = (int)Math.Ceiling((double)TotalItems / pageSize);
        ItemsFrom = pageSize * (CurrentPage - 1) + 1;
        ItemsTo = ItemsFrom + pageSize - 1;
    }
}