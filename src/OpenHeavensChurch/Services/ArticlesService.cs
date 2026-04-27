using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace OpenHeavensChurch.Services;

/// <summary>
/// Articles can either come from Umbraco (children of a "ArticlesPage" listing
/// document type with a known set of properties) or from an external JSON feed
/// in a simple, well-documented schema. The service unifies both into the
/// <see cref="Article"/> model so views can be source-agnostic.
/// </summary>
public class ArticlesService : IArticlesService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ArticlesService> _logger;

    public ArticlesService(
        IHttpClientFactory httpFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<ArticlesService> logger)
    {
        _httpFactory = httpFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ArticlePage> GetArticlesAsync(ArticleQuery query, CancellationToken cancellationToken)
    {
        var all = query.Source == ArticleSource.External
            ? await LoadExternalAsync(query.ExternalFeedUrl, cancellationToken)
            : LoadFromUmbraco(query.ListingPage);

        IEnumerable<Article> filtered = all;

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            filtered = filtered.Where(a =>
                a.Tags.Any(t => string.Equals(t, query.Tag, StringComparison.OrdinalIgnoreCase)));
        }

        var ordered = filtered
            .OrderByDescending(a => a.PublishedAt ?? DateTime.MinValue)
            .ToList();

        var pageSize = Math.Max(1, query.PageSize);
        var page = Math.Max(1, query.Page);
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new ArticlePage
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = ordered.Count
        };
    }

    public async Task<IReadOnlyList<string>> GetAllTagsAsync(ArticleSource source, string? feedUrlOverride, CancellationToken cancellationToken)
    {
        IReadOnlyList<Article> all = source == ArticleSource.External
            ? await LoadExternalAsync(feedUrlOverride, cancellationToken)
            : Array.Empty<Article>(); // Tag list for Umbraco articles is computed in the view from in-context articles.

        return all.SelectMany(a => a.Tags)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
    }

    private static IReadOnlyList<Article> LoadFromUmbraco(IPublishedContent? listingPage)
    {
        if (listingPage is null)
        {
            return Array.Empty<Article>();
        }

        return listingPage.Children
            .Where(c => c.IsVisible() && c.ContentType.Alias == "articlePage")
            .Select(MapUmbraco)
            .ToList();
    }

    private static Article MapUmbraco(IPublishedContent c)
    {
        var image = c.Value<IPublishedContent>("heroImage");
        var tags = c.Value<IEnumerable<string>>("tags") ?? Enumerable.Empty<string>();

        return new Article(
            Id: c.Key.ToString(),
            Title: c.Value<string>("title") ?? c.Name ?? "",
            Excerpt: c.Value<string>("excerpt"),
            Url: c.Url(),
            ImageUrl: image?.Url(),
            Author: c.Value<string>("author"),
            PublishedAt: c.Value<DateTime?>("publishDate") ?? c.CreateDate,
            Tags: tags.ToList(),
            Body: null);
    }

    private async Task<IReadOnlyList<Article>> LoadExternalAsync(string? feedUrlOverride, CancellationToken cancellationToken)
    {
        var feedUrl = !string.IsNullOrWhiteSpace(feedUrlOverride)
            ? feedUrlOverride
            : _configuration["OpenHeavensChurch:Articles:ExternalFeedUrl"];

        if (string.IsNullOrWhiteSpace(feedUrl))
        {
            return Array.Empty<Article>();
        }

        var cacheMinutes = _configuration.GetValue("OpenHeavensChurch:Articles:CacheMinutes", 30);
        var cacheKey = $"articles:{feedUrl}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Article>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var client = _httpFactory.CreateClient();
            var dto = await client.GetFromJsonAsync<ArticleFeedDto>(feedUrl, cancellationToken);
            var articles = (dto?.Articles ?? new List<ArticleDto>())
                .Select(MapDto)
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<Article>)articles, TimeSpan.FromMinutes(cacheMinutes));

            return articles;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load external articles feed from {Url}", feedUrl);
            return Array.Empty<Article>();
        }
    }

    private static Article MapDto(ArticleDto dto)
    {
        return new Article(
            Id: dto.Id ?? Guid.NewGuid().ToString("N"),
            Title: dto.Title ?? "Untitled",
            Excerpt: dto.Excerpt,
            Url: dto.Url ?? "#",
            ImageUrl: dto.ImageUrl,
            Author: dto.Author,
            PublishedAt: dto.PublishedAt,
            Tags: dto.Tags ?? new List<string>(),
            Body: dto.Body);
    }

    private record ArticleFeedDto(
        [property: JsonPropertyName("articles")] List<ArticleDto>? Articles);

    private record ArticleDto(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("excerpt")] string? Excerpt,
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("imageUrl")] string? ImageUrl,
        [property: JsonPropertyName("author")] string? Author,
        [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
        [property: JsonPropertyName("tags")] List<string>? Tags,
        [property: JsonPropertyName("body")] string? Body);
}
