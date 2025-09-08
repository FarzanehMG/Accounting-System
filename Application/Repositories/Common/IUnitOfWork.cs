using Application.Repositories.AccountRepositories;
using Application.Repositories.FinancialPeriod;

namespace Application.Repositories.Common;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
	IGroupAccountRepository GroupAccountRepository { get; }
    IGeneralLedgerAccountRepository GeneralLedgerAccountRepository { get; }
    ISubLedgerAccountRepository SubLedgerAccountRepository { get; }
    IDetailedAccountRepository DetailedAccountRepository { get; }
    IFinancialPeriodRepository FinancialPeriodRepository { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
    bool HasChanges<TEntity>(TEntity entity) where TEntity : class;
}