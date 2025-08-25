using Application.Features.Accounts.Queries.GroupAccount.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IGroupAccountRepository
{
	Task CreateAsync(GroupAccountEntity groupAccountEntity, CancellationToken cancellationToken = default);
	Task<GroupAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<GroupAccountEntity?> FindActiveAccountByTitleAndTypeAsync(string accountTitle, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GroupAccountEntity>> GetAllAsync(GroupAccountFilter groupAccountFilter, CancellationToken cancellationToken = default);
}