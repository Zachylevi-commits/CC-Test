using Umbraco.Cms.Core.Models.PublishedContent;

namespace OpenHeavensChurch.Models;

public enum ArticleSource
{
    Umbraco,
    External
}

public class ArticleQuery
{
    public ArticleSource Source { get; set; } = ArticleSource.Umbraco;
    public IPublishedContent? ListingPage { get; set; }
    public string? ExternalFeedUrl { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Tag { get; set; }
}

public class ArticlePage
{
    public IReadOnlyList<Article> Items { get; set; } = Array.Empty<Article>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
