using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

public interface IArticlesService
{
    Task<ArticlePage> GetArticlesAsync(ArticleQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetAllTagsAsync(ArticleSource source, string? feedUrlOverride, CancellationToken cancellationToken);
}
