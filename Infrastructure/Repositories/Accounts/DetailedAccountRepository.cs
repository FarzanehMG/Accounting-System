using Application.Features.Accounts.Queries.DetailedAccount.Dtos;
using Application.Repositories.AccountRepositories;
using Domain.Entities.Accounts;
using Domain.Enums;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Accounts;

internal class DetailedAccountRepository(AccountingDbContext dbContext) : BaseRepository<DetailedAccountEntity>(dbContext), IDetailedAccountRepository
{
    public Task CreateAsync(DetailedAccountEntity detailedAccountEntity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DetailedAccountEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DetailedAccountEntity>> FindAccountByTitleAndSubLedgerAndTypeAsync(string title, Guid subLedgerAccountId,
        JournalEntryType journalEntryType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DetailedAccountEntity?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DetailedAccountEntity>> GetAllAsync(DetailedAccountFilter detailedAccountFilter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}