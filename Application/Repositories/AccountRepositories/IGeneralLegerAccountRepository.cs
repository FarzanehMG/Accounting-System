using Domain.Entities;

namespace Application.Repositories.AccountRepositories;

public interface IGeneralLegerAccountRepository
{
    Task CreateAsync(GeneralLedgerAccountEntity generalLedgerAccountEntity, CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GeneralLedgerAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}