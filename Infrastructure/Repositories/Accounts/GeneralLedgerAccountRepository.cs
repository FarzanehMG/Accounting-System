using Application.Features.Accounts.Queries.GeneralLedgerAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Accounts;

internal class GeneralLedgerAccountRepository(AccountingDbContext dbContext) : BaseRepository<GeneralLedgerAccountEntity>(dbContext), IGeneralLedgerAccountRepository
{
    public Task CreateAsync(GeneralLedgerAccountEntity generalLedgerAccountEntity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindAccountByTitleAndGroupAndTypeAsync(string title, Guid groupAccountId, JournalEntryType journalEntryType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<GeneralLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<GeneralLedgerAccountEntity>> FindActiveAccountByTitleAndGroupAsync(string title, Guid groupAccountId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<GeneralLedgerAccountEntity>> GetAllAsync(GeneralLedgerAccountFilter generalLedgerAccountFilter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}