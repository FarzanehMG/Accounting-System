using Domain.Entities;

namespace Application.Repositories.JournalRepositories;

public interface IJournalEntryRepository
{
    Task CreateAsync(JournalEntryEntity entity, CancellationToken cancellationToken = default);
    Task<JournalEntryEntity?> GetLastEntryByVoucherIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default);
}