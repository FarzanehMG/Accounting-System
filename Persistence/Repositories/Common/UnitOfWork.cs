using Application.Common;
using Application.Repositories.AccountRepositories;
using Application.Repositories.Attachment;
using Application.Repositories.Balance;
using Application.Repositories.Common;
using Application.Repositories.FinancialPeriod;
using Application.Repositories.JournalRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repositories.Accounts;
using Persistence.Repositories.Attachment;
using Persistence.Repositories.Balance;
using Persistence.Repositories.FinancialPeriod;
using Persistence.Repositories.Journal;
using System.Data;

namespace Persistence.Repositories.Common;

public class UnitOfWork(AccountingDbContext dbContext) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    public void Dispose()
    {
        _transaction?.Dispose();
        dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await dbContext.DisposeAsync();
    }

    public IGroupAccountRepository GroupAccountRepository { get; } = new GroupAccountRepository(dbContext);
    public IGeneralLedgerAccountRepository GeneralLedgerAccountRepository { get; } = new GeneralLedgerAccountRepository(dbContext);
    public ISubLedgerAccountRepository SubLedgerAccountRepository { get; } = new SubLedgerAccountRepository(dbContext);
    public IDetailedAccountRepository DetailedAccountRepository { get; } = new DetailedAccountRepository(dbContext);

    public IFinancialPeriodRepository FinancialPeriodRepository { get; } = new FinancialPeriodRepository(dbContext);

    public IJournalVoucherRepository JournalVoucherRepository { get; } = new JournalVoucherRepository(dbContext);
    public IJournalEntryRepository JournalEntryRepository { get; } = new JournalEntryRepository(dbContext);

    public IAttachmentRepository AttachmentRepository { get; } = new AttachmentRepository(dbContext);

    public IAccountBalanceRepository AccountBalanceRepository { get; } = new AccountBalanceRepository(dbContext);

    public async Task BeginTransactionAsync(TransactionIsolation level, CancellationToken cancellationToken = default)
    {
        var efLevel = MapToEf(level);
        _transaction = await dbContext.Database.BeginTransactionAsync(efLevel, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private static IsolationLevel MapToEf(TransactionIsolation level)
    {
        return level switch
        {
            TransactionIsolation.RepeatableRead => IsolationLevel.RepeatableRead,
            TransactionIsolation.Serializable => IsolationLevel.Serializable,
            _ => IsolationLevel.ReadCommitted
        };
    }
}