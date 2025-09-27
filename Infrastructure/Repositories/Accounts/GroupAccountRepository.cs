using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Accounts;

internal class GroupAccountRepository(AccountingDbContext dbContext) : BaseRepository<GroupAccountEntity>(dbContext), IGroupAccountRepository
{
    public Task CreateAsync(GroupAccountEntity groupAccountEntity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GroupAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GroupAccountEntity?> FindActiveAccountByTitleAndTypeAsync(string accountTitle, JournalEntryType journalEntryType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<GroupAccountEntity>> GetAllAsync(GroupAccountFilter groupAccountFilter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}