namespace OpenHeavensChurch.Models;

public record Article(
    string Id,
    string Title,
    string? Excerpt,
    string Url,
    string? ImageUrl,
    string? Author,
    DateTime? PublishedAt,
    IReadOnlyList<string> Tags,
    string? Body);
