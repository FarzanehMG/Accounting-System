using Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;
using Domain.Entities.Accounts;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface ISubLedgerAccountRepository
{
    Task CreateAsync(SubLedgerAccountEntity entity, CancellationToken cancellationToken = default);
    Task<SubLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAndTypeAsync(string title, Guid generalLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAccountAsync(string title, Guid generalLedgerAccountId, CancellationToken cancellationToken = default);
    Task<SubLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubLedgerAccountEntity>> GetAllAsync(SubLedgerAccountFilter filter, CancellationToken cancellationToken = default);
}