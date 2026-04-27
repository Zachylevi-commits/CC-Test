using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;
using Umbraco.Cms.Infrastructure.Scoping;

namespace OpenHeavensChurch.Services;

public class ContactSubmissionService : IContactSubmissionService
{
    private readonly IScopeProvider _scopeProvider;
    private readonly ILogger<ContactSubmissionService> _logger;

    public ContactSubmissionService(IScopeProvider scopeProvider, ILogger<ContactSubmissionService> logger)
    {
        _scopeProvider = scopeProvider;
        _logger = logger;
    }

    public Task<int> SaveAsync(ContactSubmission submission, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var db = scope.Database;

        // Inserts and assigns the auto-increment id back to the model.
        db.Insert(submission);

        scope.Complete();

        _logger.LogInformation("Saved contact submission {Id} from {Email}", submission.Id, submission.Email);

        return Task.FromResult(submission.Id);
    }

    public Task<IReadOnlyList<ContactSubmission>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeProvider.CreateScope(autoComplete: true);
        var sql = scope.Database.SqlContext.Sql()
            .Select<ContactSubmission>()
            .From<ContactSubmission>()
            .OrderBy<ContactSubmission>(x => x.SubmittedAt, NPoco.OrderDirection.Descending);

        var results = scope.Database.Fetch<ContactSubmission>(sql).Take(count).ToList();

        return Task.FromResult<IReadOnlyList<ContactSubmission>>(results);
    }
}
