using Application.Features.Accounts.Queries.DetailedAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Accounts;

internal sealed class DetailedAccountRepository(AccountingDbContext dbContext) : BaseRepository<DetailedAccountEntity>(dbContext), IDetailedAccountRepository
{
    public async Task CreateAsync(DetailedAccountEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAndTypeAsync(string title, Guid subLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.SubLedgerAccountId == subLedgerAccountId &&
                x.JournalEntryType == journalEntryType)
            .ToListAsync(cancellationToken);
    }

    public async Task<DetailedAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.AccountCode.Equals(code), cancellationToken);
    }

    public async Task<IReadOnlyList<DetailedAccountEntity>> GetAllAsync(DetailedAccountFilter filter, CancellationToken cancellationToken = default)
    {
        var query = TableNoTracking;

        if (!string.IsNullOrEmpty(filter.Title))
            query = query.Where(x => x.AccountTitle.Equals(filter.Title));

        if (!string.IsNullOrEmpty(filter.Code))
            query = query.Where(x => x.AccountCode.Equals(filter.Code));

        if (filter.JournalEntryType.HasValue)
            query = query.Where(x => x.JournalEntryType == filter.JournalEntryType.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        if (filter.SubLedgerAccountId.HasValue)
            query = query.Where(x => x.SubLedgerAccountId == filter.SubLedgerAccountId.Value);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }
}