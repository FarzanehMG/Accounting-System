using Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;
using Domain.Entities.Accounts;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IGeneralLedgerAccountRepository
{
    Task CreateAsync(GeneralLedgerAccountEntity entity, CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAndTypeAsync(string title, Guid groupAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<GeneralLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAccountAsync(string title, Guid groupAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountEntity>> GetAllAsync(GeneralLedgerAccountFilter filter, CancellationToken cancellationToken = default);
}