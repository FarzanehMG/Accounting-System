using Application.Repositories.AccountRepositories;

namespace Application.Repositories.Common;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
	IGroupAccountRepository GroupAccountRepository { get; }
    IGeneralLegerAccountRepository GeneralLegerAccountRepository { get; }
    ISubLegerAccountRepository SubLegerAccountRepository { get; }
    IDetailedAccountRepository DetailedAccountRepository { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}