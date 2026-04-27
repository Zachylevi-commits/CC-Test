namespace OpenHeavensChurch.Models;

public record Song(
    string Id,
    string Title,
    string Artist,
    string ArtworkUrl,
    DateTime? ReleasedAt,
    IReadOnlyList<SongPlatformLink> Platforms);

public record SongPlatformLink(string Platform, string DisplayName, string Url);
