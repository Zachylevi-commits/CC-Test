using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

public interface IYouTubeService
{
    Task<IReadOnlyList<YouTubeVideo>> GetLatestVideosAsync(int maxResults, string? channelIdOverride, CancellationToken cancellationToken);
}
