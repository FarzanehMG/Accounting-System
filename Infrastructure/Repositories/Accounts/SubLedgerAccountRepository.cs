using Application.Features.Accounts.Queries.SubLedgerAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Accounts;

internal class SubLedgerAccountRepository(AccountingDbContext dbContext) : BaseRepository<SubLedgerAccountEntity>(dbContext), ISubLedgerAccountRepository
{
    public Task CreateAsync(SubLedgerAccountEntity subLedgerAccountEntity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SubLedgerAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SubLedgerAccountEntity>> FindAccountByTitleAndGeneralLedgerAndTypeAsync(string title, Guid generalLedgerAccountId,
        JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SubLedgerAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SubLedgerAccountEntity>> GetAllAsync(SubLedgerAccountFilter subLedgerAccountFilter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}