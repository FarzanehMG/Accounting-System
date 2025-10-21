using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Accounts;

internal sealed class GroupAccountRepository(AccountingDbContext dbContext) : BaseRepository<GroupAccountEntity>(dbContext), IGroupAccountRepository
{
    public async Task CreateAsync(GroupAccountEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }

    public async Task<GroupAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Table.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<GroupAccountEntity?> FindActiveAccountByTitleAndTypeAsync(string accountTitle, JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        return await TableNoTracking.FirstOrDefaultAsync(x =>
            x.AccountTitle.Equals(accountTitle) &&
            x.JournalEntryType == journalEntryType &&
            x.IsActive,
            cancellationToken);
    }

    public async Task<IReadOnlyList<GroupAccountEntity>> GetAllAsync(GroupAccountFilter filter, CancellationToken cancellationToken = default)
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