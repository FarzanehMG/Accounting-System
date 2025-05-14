using Domain.Entities;

namespace Application.Repositories.AccountRepositories;

public interface IGroupAccountRepository
{
	Task CreateAsync(GroupAccount groupAccount, CancellationToken cancellationToken = default);
	Task<GroupAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<GroupAccount?> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}