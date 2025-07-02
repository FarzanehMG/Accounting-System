using Domain.Entities;

namespace Application.Repositories.AccountRepositories;

public interface IDetailedAccountRepository
{
    Task CreateAsync(DetailedAccountEntity detailedAccountEntity, CancellationToken cancellationToken = default);
    Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DetailedAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}