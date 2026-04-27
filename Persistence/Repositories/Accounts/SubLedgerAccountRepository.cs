using Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Accounts;

internal sealed class SubLedgerAccountRepository(AccountingDbContext dbContext) : BaseRepository<SubLedgerAccountEntity>(dbContext), ISubLedgerAccountRepository
{
    public async Task CreateAsync(SubLedgerAccountEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<SubLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(x=>x.GeneralLedgerAccount)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SubLedgerAccountEntity>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await TableNoTracking
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAndTypeAsync(string title, Guid generalLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.GeneralLedgerAccountId == generalLedgerAccountId &&
                x.JournalEntryType == journalEntryType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAccountAsync(string title, Guid generalLedgerAccountId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.GeneralLedgerAccountId == generalLedgerAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SubLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.AccountCode.Equals(code), cancellationToken);
    }

    public async Task<IReadOnlyList<SubLedgerAccountEntity>> GetAllAsync(SubLedgerAccountFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking.AsQueryable();

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(x => x.AccountTitle.Contains(filter.Title));

        if (!string.IsNullOrEmpty(filter.Code))
            query = query.Where(x => x.AccountCode.Contains(filter.Code));

        if (filter.JournalEntryType.HasValue)
            query = query.Where(x => x.JournalEntryType == filter.JournalEntryType.Value);

        // TODO: Apply pagination
        return await query.OrderBy(x => x.Id).ToListAsync(cancellationToken);
    }
}