using Application.Features.Journal.Queries.JournalEntry.Dtos;
using Domain.Entities.Journal;

namespace Application.Repositories.JournalRepositories;

public interface IJournalEntryRepository
{
    Task CreateAsync(JournalEntryEntity entity, CancellationToken cancellationToken = default);
    Task<JournalEntryEntity?> GetLastEntryByVoucherIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default);
    public Task<JournalEntryEntity?> GetByIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntryEntity>> GetAllAsync(JournalEntryFilter filter, CancellationToken cancellationToken = default);
}