using Application.Repositories.Balance;
using Domain.Entities.Balance;
using Persistence.Repositories.Common;

namespace Persistence.Repositories.Balance;

internal sealed class AccountBalanceRepository(AccountingDbContext dbContext) : BaseRepository<AccountBalanceEntity>(dbContext), IAccountBalanceRepository
{
    public async Task CreateAsync(AccountBalanceEntity entity, CancellationToken cancellationToken = default)
    {
        await AddAsync(entity, cancellationToken);
    }
}