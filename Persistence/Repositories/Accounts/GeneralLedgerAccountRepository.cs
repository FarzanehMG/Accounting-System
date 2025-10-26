using Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Accounts;

internal sealed class GeneralLedgerAccountRepository(AccountingDbContext dbContext) : BaseRepository<GeneralLedgerAccountEntity>(dbContext), IGeneralLedgerAccountRepository
{
    public async Task CreateAsync(GeneralLedgerAccountEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<GeneralLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAndTypeAsync(string title, Guid groupAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.JournalEntryType == journalEntryType &&
                x.GroupAccountId == groupAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<GeneralLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x => x.AccountCode.Equals(code), cancellationToken);
    }

    public async Task<GeneralLedgerAccountEntity?> FindByCodeAndGroupAccountIdAsync(string code, Guid groupAccountId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking
            .FirstOrDefaultAsync(x => x.AccountCode.Equals(code) && 
                x.GroupAccountId == groupAccountId, 
                cancellationToken);
    }

    public async Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAccountAsync(string title, Guid groupAccountId, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.Where(x =>
                x.AccountTitle.Equals(title) &&
                x.GroupAccountId == groupAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GeneralLedgerAccountEntity>> GetAllAsync(GeneralLedgerAccountFilter filter, CancellationToken cancellationToken = default)
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

        if (filter.GroupAccountId.HasValue)
            query = query.Where(x => x.GroupAccountId == filter.GroupAccountId.Value);

        // TODO: Apply pagination
        return await query.ToListAsync(cancellationToken);
    }
}