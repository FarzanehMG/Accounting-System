using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Domain.Entities.Accounts;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IGroupAccountRepository
{
	Task CreateAsync(GroupAccountEntity entity, CancellationToken cancellationToken = default);
	Task<GroupAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<GroupAccountEntity>> FindAccountByTitleAndTypeAsync(string accountTitle, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<GroupAccountEntity>> GetAllAsync(GroupAccountFilter filter, CancellationToken cancellationToken = default);
    Task<GroupAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}