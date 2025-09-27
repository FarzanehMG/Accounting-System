using Application.Features.Journal.Queries.JournalEntry.Dtos;
using Application.Repositories.JournalRepositories;
using Domain.Entities.Journal;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Journal;

internal class JournalEntryRepository(AccountingDbContext dbContext) : BaseRepository<JournalEntryEntity>(dbContext), IJournalEntryRepository
{
    public Task CreateAsync(JournalEntryEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JournalEntryEntity?> GetLastEntryByVoucherIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<JournalEntryEntity?> GetByIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<JournalEntryEntity>> GetAllAsync(JournalEntryFilter filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}