using Domain.Entities.Balance;

namespace Application.Repositories.Balance;

public interface IAccountBalanceRepository
{
    Task CreateAsync(AccountBalanceEntity entity, CancellationToken cancellationToken = default);
}