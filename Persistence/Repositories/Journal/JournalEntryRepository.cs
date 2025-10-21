using Application.Features.Journal.Queries.JournalEntry.Dtos;
using Application.Repositories.JournalRepositories;
using Domain.Entities.Journal;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Journal;

internal sealed class JournalEntryRepository(AccountingDbContext dbContext) : BaseRepository<JournalEntryEntity>(dbContext), IJournalEntryRepository
{
    public async Task CreateAsync(JournalEntryEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<JournalEntryEntity?> GetLastEntryByVoucherIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking
            .Where(x => x.JournalVoucherId == journalEntryId)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<JournalEntryEntity?> GetByIdAsync(Guid journalEntryId, CancellationToken cancellationToken = default)
    {
        return await Table.FirstOrDefaultAsync(x => x.Id == journalEntryId, cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntryEntity>> GetAllAsync(JournalEntryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking;

        if (!string.IsNullOrEmpty(filter.JournalEntryNumber))
            query = query.Where(x => x.JournalEntryNumber.Equals(filter.JournalEntryNumber));

        if (filter.JournalEntryType.HasValue)
            query = query.Where(x => x.JournalEntryType == filter.JournalEntryType.Value);

        if (filter.Amount.HasValue)
            query = query.Where(x => x.Amount == filter.Amount.Value);

        if (filter.JournalVoucherId.HasValue)
            query = query.Where(x => x.JournalVoucherId == filter.JournalVoucherId.Value);

        if (filter.DetailedAccountId.HasValue)
            query = query.Where(x => x.DetailedAccountId == filter.DetailedAccountId.Value);

        if (filter.SubLedgerAccountId.HasValue)
            query = query.Where(x => x.SubLedgerAccountId == filter.SubLedgerAccountId.Value);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyByGroupAccountIdAsync(Guid groupAccountId, CancellationToken cancellationToken = default)
    {
        var subLedgerUsed = await TableNoTracking
            .AnyAsync(e => e.SubLedgerAccount!.GeneralLedgerAccount!.GroupAccountId == groupAccountId, cancellationToken);

        if (subLedgerUsed)
            return true;

        var detailedUsed = await TableNoTracking
            .AnyAsync(e => e.DetailedAccount!.SubLedgerAccount!.GeneralLedgerAccount!.GroupAccountId == groupAccountId, cancellationToken);

        return detailedUsed;
    }
}