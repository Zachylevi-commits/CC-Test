using OpenHeavensChurch.Models;

namespace OpenHeavensChurch.Services;

public interface IContactSubmissionService
{
    Task<int> SaveAsync(ContactSubmission submission, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactSubmission>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
}
