namespace OpenHeavensChurch.Models;

public record YouTubeVideo(
    string VideoId,
    string Title,
    string Description,
    string ThumbnailUrl,
    DateTime PublishedAt,
    string ChannelTitle)
{
    public string WatchUrl => $"https://www.youtube.com/watch?v={VideoId}";

    public string ShortDescription(int maxLength)
    {
        if (string.IsNullOrEmpty(Description) || Description.Length <= maxLength)
        {
            return Description ?? string.Empty;
        }
        return Description.AsSpan(0, maxLength).TrimEnd().ToString() + "…";
    }
}
