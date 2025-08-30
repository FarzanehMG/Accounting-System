using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface ISubLegerAccountRepository
{
    Task CreateAsync(SubLedgerAccountEntity subLedgerAccountEntity, CancellationToken cancellationToken = default);
    Task<SubLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubLedgerAccountEntity>> FindActiveAccountByTitleAndGeneralAsync(string title, Guid generalLedgerAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAndTypeAsync(string title, Guid generalLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<SubLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<SubLedgerAccountEntity>> GetByTitleAsync(string accountTitle, CancellationToken cancellationToken = default);
}