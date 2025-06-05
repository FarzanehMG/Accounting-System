using Domain.Entities;

namespace Application.Repositories.AccountRepositories;

public interface IGroupAccountRepository
{
	Task CreateAsync(GroupAccountEntity groupAccountEntity, CancellationToken cancellationToken = default);
	Task<GroupAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<GroupAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}