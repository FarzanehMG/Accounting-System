using Application.Common;
using Application.Repositories.AccountRepositories;
using Application.Repositories.Attachment;
using Application.Repositories.Balance;
using Application.Repositories.FinancialPeriod;
using Application.Repositories.JournalRepositories;

namespace Application.Repositories.Common;

public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
	IGroupAccountRepository GroupAccountRepository { get; }
    IGeneralLedgerAccountRepository GeneralLedgerAccountRepository { get; }
    ISubLedgerAccountRepository SubLedgerAccountRepository { get; }
    IDetailedAccountRepository DetailedAccountRepository { get; }
    IFinancialPeriodRepository FinancialPeriodRepository { get; }
    IJournalVoucherRepository JournalVoucherRepository { get; }
    IJournalEntryRepository JournalEntryRepository { get; }
    IAttachmentRepository AttachmentRepository { get; }
    IAccountBalanceRepository AccountBalanceRepository { get; }

    Task BeginTransactionAsync(TransactionIsolation level = TransactionIsolation.Default, CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
}