using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

public class YouTubeService : IYouTubeService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<YouTubeService> _logger;

    public YouTubeService(
        IHttpClientFactory httpFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<YouTubeService> logger)
    {
        _httpFactory = httpFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<YouTubeVideo>> GetLatestVideosAsync(
        int maxResults,
        string? channelIdOverride,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["OpenHeavensChurch:YouTube:ApiKey"];
        var channelId = string.IsNullOrWhiteSpace(channelIdOverride)
            ? _configuration["OpenHeavensChurch:YouTube:ChannelId"]
            : channelIdOverride;
        var cacheMinutes = _configuration.GetValue("OpenHeavensChurch:YouTube:CacheMinutes", 30);

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(channelId)
            || apiKey.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase)
            || channelId.StartsWith("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("YouTube API not configured. Returning empty result.");
            return Array.Empty<YouTubeVideo>();
        }

        var cacheKey = $"yt:{channelId}:{maxResults}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<YouTubeVideo>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var client = _httpFactory.CreateClient();
            var url = $"https://www.googleapis.com/youtube/v3/search" +
                      $"?part=snippet&order=date&type=video&maxResults={maxResults}" +
                      $"&channelId={Uri.EscapeDataString(channelId)}" +
                      $"&key={Uri.EscapeDataString(apiKey)}";

            var response = await client.GetFromJsonAsync<YouTubeSearchResponse>(url, cancellationToken);
            var items = response?.Items ?? Array.Empty<YouTubeSearchItem>();

            var videos = items
                .Where(i => i.Id?.VideoId is not null)
                .Select(i => new YouTubeVideo(
                    VideoId: i.Id!.VideoId!,
                    Title: i.Snippet?.Title ?? "",
                    Description: i.Snippet?.Description ?? "",
                    ThumbnailUrl: i.Snippet?.Thumbnails?.High?.Url
                                  ?? i.Snippet?.Thumbnails?.Medium?.Url
                                  ?? i.Snippet?.Thumbnails?.Default_?.Url
                                  ?? "",
                    PublishedAt: i.Snippet?.PublishedAt ?? DateTime.MinValue,
                    ChannelTitle: i.Snippet?.ChannelTitle ?? ""))
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<YouTubeVideo>)videos, TimeSpan.FromMinutes(cacheMinutes));
            return videos;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch YouTube feed for channel {Channel}", channelId);
            return Array.Empty<YouTubeVideo>();
        }
    }

    private record YouTubeSearchResponse(
        [property: JsonPropertyName("items")] List<YouTubeSearchItem>? Items);

    private record YouTubeSearchItem(
        [property: JsonPropertyName("id")] YouTubeIdRef? Id,
        [property: JsonPropertyName("snippet")] YouTubeSnippet? Snippet);

    private record YouTubeIdRef(
        [property: JsonPropertyName("videoId")] string? VideoId);

    private record YouTubeSnippet(
        [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("channelTitle")] string? ChannelTitle,
        [property: JsonPropertyName("thumbnails")] YouTubeThumbnailSet? Thumbnails);

    private record YouTubeThumbnailSet(
        [property: JsonPropertyName("default")] YouTubeThumbnail? Default_,
        [property: JsonPropertyName("medium")] YouTubeThumbnail? Medium,
        [property: JsonPropertyName("high")] YouTubeThumbnail? High);

    private record YouTubeThumbnail(
        [property: JsonPropertyName("url")] string? Url);
}
