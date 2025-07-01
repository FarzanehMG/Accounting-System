using Domain.Entities;

namespace Application.Repositories.AccountRepositories;

public interface ISubLegerAccountRepository
{
    Task CreateAsync(SubLedgerAccountEntity subLedgerAccountEntity, CancellationToken cancellationToken = default);
    Task<SubLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubLedgerAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}