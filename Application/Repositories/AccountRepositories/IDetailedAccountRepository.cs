using Application.Features.Accounts.Queries.DetailedAccount.Dtos;
using Domain.Entities.Accounts;
using Domain.Enums;

namespace Application.Repositories.AccountRepositories;

public interface IDetailedAccountRepository
{
    Task CreateAsync(DetailedAccountEntity entity, CancellationToken cancellationToken = default);
    Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAndTypeAsync(string title, Guid subLedgerAccountId, JournalEntryType journalEntryType, CancellationToken cancellationToken = default);
    Task<DetailedAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DetailedAccountEntity>> GetAllAsync(DetailedAccountFilter filter, CancellationToken cancellationToken = default);
}