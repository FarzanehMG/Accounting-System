using Application.Repositories.AccountRepositories;

namespace Application.Repositories.Common;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
	IGroupAccountRepository GroupAccountRepository { get; }
    IGeneralLegerAccountRepository GeneralLegerAccountRepository { get; }

    Task CommitAsync(CancellationToken cancellationToken = default);
}