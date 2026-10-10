using Application.Features.Accounts.Queries.DetailedAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Accounts;

internal sealed class DetailedAccountRepository(AccountingDbContext dbContext) : BaseRepository<DetailedAccountEntity>(dbContext), IDetailedAccountRepository
{
    public async Task CreateAsync(DetailedAccountEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(x=>x.SubLedgerAccount)
            .Include(x=>x.SubLedgerAccount!.GeneralLedgerAccount)
            .Include(x=>x.SubLedgerAccount!.GeneralLedgerAccount!.GroupAccount)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await TableNoTracking
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAndTypeAsync(string title, Guid subLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.SubLedgerAccountId == subLedgerAccountId &&
                x.JournalEntryType == journalEntryType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAccountAsync(string title, Guid subLedgerAccountId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.SubLedgerAccountId == subLedgerAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DetailedAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.AccountCode.Equals(code), cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> GetAllAsync(DetailedAccountFilter filter, CancellationToken cancellationToken = default)
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