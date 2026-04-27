using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

public interface ISongsService
{
    Task<IReadOnlyList<Song>> GetLatestSongsAsync(int maxResults, string? feedUrlOverride, CancellationToken cancellationToken);
}
