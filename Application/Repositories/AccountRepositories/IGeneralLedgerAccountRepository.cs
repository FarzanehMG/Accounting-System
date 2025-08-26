using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IGeneralLedgerAccountRepository
{
    Task CreateAsync(GeneralLedgerAccountEntity generalLedgerAccountEntity, CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAndTypeAsync(string title, Guid groupAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindActiveAccountByTitleAndGroupAsync(string title, Guid groupAccountId, CancellationToken cancellationToken = default);
    Task<List<GeneralLedgerAccountEntity>> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
}