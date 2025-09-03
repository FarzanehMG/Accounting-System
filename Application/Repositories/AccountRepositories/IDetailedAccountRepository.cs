using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IDetailedAccountRepository
{
    Task CreateAsync(DetailedAccountEntity detailedAccountEntity, CancellationToken cancellationToken = default);
    Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAndTypeAsync(string title, Guid subLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<DetailedAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<DetailedAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}