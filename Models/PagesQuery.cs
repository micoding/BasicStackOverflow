namespace BasicStackOverflow.Models;

public class PagesQuery
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public string SearchString { get; set; }
    public int? SkipForPage => PageSize * (PageNumber - 1);

    public string SortBy { get; set; }

    public bool? SortAscending { get; set; }
}