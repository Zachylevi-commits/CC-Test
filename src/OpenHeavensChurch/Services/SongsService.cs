using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

/// <summary>
/// Loads the church's latest songs from a JSON feed. The feed URL can come from
/// appsettings (default) or be overridden per page/block via Umbraco. Each song
/// can advertise links to multiple streaming platforms which are surfaced in the
/// per-song "Listen on…" menu in the UI.
/// </summary>
public class SongsService : ISongsService
{
    private static readonly Dictionary<string, string> KnownPlatformDisplayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["spotify"] = "Spotify",
        ["apple"] = "Apple Music",
        ["appleMusic"] = "Apple Music",
        ["youtube"] = "YouTube",
        ["youtubeMusic"] = "YouTube Music",
        ["amazon"] = "Amazon Music",
        ["amazonMusic"] = "Amazon Music",
        ["deezer"] = "Deezer",
        ["tidal"] = "Tidal",
        ["soundcloud"] = "SoundCloud",
        ["bandcamp"] = "Bandcamp",
        ["pandora"] = "Pandora"
    };

    private readonly IHttpClientFactory _httpFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SongsService> _logger;

    public SongsService(
        IHttpClientFactory httpFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<SongsService> logger)
    {
        _httpFactory = httpFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Song>> GetLatestSongsAsync(
        int maxResults,
        string? feedUrlOverride,
        CancellationToken cancellationToken)
    {
        var feedUrl = !string.IsNullOrWhiteSpace(feedUrlOverride)
            ? feedUrlOverride
            : _configuration["OpenHeavensChurch:Songs:FeedUrl"];

        if (string.IsNullOrWhiteSpace(feedUrl))
        {
            return Array.Empty<Song>();
        }

        var cacheMinutes = _configuration.GetValue("OpenHeavensChurch:Songs:CacheMinutes", 60);
        var cacheKey = $"songs:{feedUrl}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Song>? cached) && cached is not null)
        {
            return cached.Take(maxResults).ToList();
        }

        try
        {
            var client = _httpFactory.CreateClient();
            var dto = await client.GetFromJsonAsync<SongFeedDto>(feedUrl, cancellationToken);

            var songs = (dto?.Songs ?? new List<SongDto>())
                .Select(MapSong)
                .OrderByDescending(s => s.ReleasedAt ?? DateTime.MinValue)
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<Song>)songs, TimeSpan.FromMinutes(cacheMinutes));

            return songs.Take(maxResults).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch songs feed from {Url}", feedUrl);
            return Array.Empty<Song>();
        }
    }

    private static Song MapSong(SongDto dto)
    {
        var platforms = (dto.Platforms ?? new List<SongPlatformDto>())
            .Where(p => !string.IsNullOrWhiteSpace(p.Url))
            .Select(p => new SongPlatformLink(
                Platform: p.Platform ?? "unknown",
                DisplayName: KnownPlatformDisplayNames.TryGetValue(p.Platform ?? string.Empty, out var name)
                    ? name
                    : (p.DisplayName ?? p.Platform ?? "Listen"),
                Url: p.Url!))
            .ToList();

        return new Song(
            Id: dto.Id ?? Guid.NewGuid().ToString("N"),
            Title: dto.Title ?? "Untitled",
            Artist: dto.Artist ?? "Open Heavens Worship",
            ArtworkUrl: dto.Artwork ?? string.Empty,
            ReleasedAt: dto.ReleasedAt,
            Platforms: platforms);
    }

    private record SongFeedDto(
        [property: JsonPropertyName("songs")] List<SongDto>? Songs);

    private record SongDto(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("artist")] string? Artist,
        [property: JsonPropertyName("artwork")] string? Artwork,
        [property: JsonPropertyName("releasedAt")] DateTime? ReleasedAt,
        [property: JsonPropertyName("platforms")] List<SongPlatformDto>? Platforms);

    private record SongPlatformDto(
        [property: JsonPropertyName("platform")] string? Platform,
        [property: JsonPropertyName("displayName")] string? DisplayName,
        [property: JsonPropertyName("url")] string? Url);
}
